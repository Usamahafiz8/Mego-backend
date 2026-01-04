using MeGo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MeGo.Api.Services;
using MeGo.Api.Hubs;
using MeGo.Api.Filters;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// 🔥 VERY IMPORTANT (MOBILE / EXPO ACCESS)
builder.WebHost.UseUrls("http://0.0.0.0:5144");

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
// 🧠 Services
// --------------------------------------------------
builder.Services.AddScoped<RewardService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<TwilioVerifyService>();
builder.Services.AddScoped<AdQualityScoreService>();
builder.Services.AddScoped<SpamDetectionService>();
builder.Services.AddHostedService<AdRelistReminderService>();

// --------------------------------------------------
// 🗃️ Database
// --------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

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
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
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
});

// --------------------------------------------------
// 🚀 BUILD
// --------------------------------------------------
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

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
app.MapControllers();
app.MapHub<AdminHub>("/hubs/admin");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<UserHub>("/userHub");

app.MapGet("/", () => "🚀 MeGo Backend Running");

// --------------------------------------------------
app.Run();
