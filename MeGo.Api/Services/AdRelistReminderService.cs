using MeGo.Api.Data;
using MeGo.Api.Models;
using MeGo.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Services
{
    public class AdRelistReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdRelistReminderService> _logger;

        public AdRelistReminderService(IServiceProvider serviceProvider, ILogger<AdRelistReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    
                    // Get NotificationService, but handle if it fails to initialize
                    NotificationService? notificationService = null;
                    try
                    {
                        notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "NotificationService not available - continuing without push notifications");
                    }

                    // Check for inactive ads (no views/clicks in last 7 days)
                    var inactiveAds = await context.Ads
                        .Where(a => a.IsActive && a.Status == "approved" && 
                            a.CreatedAt < DateTime.UtcNow.AddDays(-7))
                        .Include(a => a.User)
                        .ToListAsync(stoppingToken);

                    foreach (var ad in inactiveAds)
                    {
                        var analytics = await context.AdAnalytics
                            .FirstOrDefaultAsync(a => a.AdId == ad.Id, stoppingToken);

                        // Check if ad has been inactive (no views in last 7 days)
                        bool isInactive = analytics == null || 
                            (analytics.LastViewedAt == null || analytics.LastViewedAt < DateTime.UtcNow.AddDays(-7));

                        if (isInactive)
                        {
                            // Check if reminder already sent
                            var reminder = await context.AdRelistReminders
                                .FirstOrDefaultAsync(r => r.AdId == ad.Id && r.IsActive, stoppingToken);

                            if (reminder == null)
                            {
                                // Create reminder
                                reminder = new AdRelistReminder
                                {
                                    AdId = ad.Id,
                                    UserId = ad.UserId,
                                    LastActiveAt = analytics?.LastViewedAt ?? ad.CreatedAt,
                                    InactiveDays = 7,
                                    IsActive = true
                                };
                                context.AdRelistReminders.Add(reminder);
                                await context.SaveChangesAsync(stoppingToken);
                            }
                            else if (reminder.ReminderSentAt == null || 
                                reminder.ReminderSentAt < DateTime.UtcNow.AddDays(-1))
                            {
                                // Send reminder notification
                                await notificationService.SendNotificationAsync(
                                    ad.UserId,
                                    "Relist Your Ad",
                                    $"Your ad '{ad.Title}' hasn't received views recently. Consider relisting it to get more visibility!",
                                    "ad_relist_reminder",
                                    ad.Id.ToString()
                                );

                                reminder.ReminderSentAt = DateTime.UtcNow;
                                await context.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation($"Sent relist reminder for ad {ad.Id}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AdRelistReminderService");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

