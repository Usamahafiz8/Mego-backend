using MeGo.Api.Data;
using MeGo.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)
            ));

        services.AddScoped<RewardService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<EmailService>();
        services.AddScoped<TwilioVerifyService>();
        services.AddScoped<AdQualityScoreService>();
        services.AddScoped<SpamDetectionService>();
        services.AddHostedService<AdRelistReminderService>();

        return services;
    }
}
