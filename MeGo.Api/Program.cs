using MeGo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MeGo.Api.Services;
using MeGo.Api.Hubs;
using MeGo.Api.Filters;
using MeGo.Api.Extensions;
using MeGo.Api.Middleware;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Serilog;

// Configure Serilog for file and console logging
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(logDirectory, "app-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Use Serilog
builder.Host.UseSerilog();

// 🔥 Configure Port (supports environment variables for cloud deployment)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5144";
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? $"http://0.0.0.0:{port}";
builder.WebHost.UseUrls(urls);

// --------------------------------------------------
// 🧩 Controllers
// --------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// --------------------------------------------------
// 🏥 Health Checks
// --------------------------------------------------
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "postgresql",
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(5));

// --------------------------------------------------
// 🧠 Services & Database
// --------------------------------------------------
builder.Services.AddApplicationServices(builder.Configuration);

// --------------------------------------------------
// 🔐 JWT AUTH
// --------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwtSettings["Key"] ?? "default-secret-key-min-32-chars"
                )
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/admin") ||
                     path.StartsWithSegments("/chatHub") ||
                     path.StartsWithSegments("/userHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// --------------------------------------------------
// 🌐 CORS (SAFE FOR MOBILE)
// --------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// --------------------------------------------------
// 📡 SignalR
// --------------------------------------------------
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

// --------------------------------------------------
// 🧾 Swagger
// --------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MeGo API",
        Version = "v1.0.0",
        Description = "Professional marketplace API for MeGo platform. Features include JWT authentication, real-time communication via SignalR, marketplace operations, wallet system, and more.",
        Contact = new OpenApiContact
        {
            Name = "MeGo API Support",
            Email = "support@mego.com.pk"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.OperationFilter<FileUploadOperationFilter>();
    
    // Ignore errors for file upload endpoints
    c.IgnoreObsoleteActions();
    c.CustomSchemaIds(type => type.FullName);
    
    // Exclude problematic action from Swagger
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Skip the problematic SubmitLiveVerification action
        if (apiDesc.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDesc)
        {
            if (actionDesc.ControllerName == "UserKyc" && 
                actionDesc.ActionName == "SubmitLiveVerification")
            {
                return false;
            }
        }
        return true;
    });
});

// --------------------------------------------------
// 🚀 BUILD
// --------------------------------------------------
var app = builder.Build();

// --------------------------------------------------
// 🛡️ Professional Middleware Pipeline
// --------------------------------------------------
app.UseProfessionalMiddleware();

// Swagger (Development only)
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeGo API v1.0.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "MeGo API Documentation";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
    });
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// 📁 Static Files
// --------------------------------------------------
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")
    ),
    RequestPath = "/uploads"
});

// --------------------------------------------------
// 📍 Routes
// --------------------------------------------------
// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// API Routes
app.MapControllers();
app.MapHub<AdminHub>("/hubs/admin");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<UserHub>("/userHub");

// Root Endpoint
app.MapGet("/", () => new
{
    Service = "MeGo API",
    Status = "Running",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow,
    Documentation = "/swagger"
});

// --------------------------------------------------
try
{
    Log.Information("🚀 MeGo API starting up...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
