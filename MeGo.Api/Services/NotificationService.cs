using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MeGo.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Services
{
    public class NotificationService
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("mego-app-776ad-firebase-adminsdk-fbsvc-7b18da4e96.json") // ✅ is file ko root me rakho
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }

        public async Task SendMulticastAsync(List<string> tokens, string title, string body, Dictionary<string, string>? data = null)
        {
            if (!tokens.Any()) return;

            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Tokens = tokens,
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            Console.WriteLine($"✅ Sent: {response.SuccessCount}, Failed: {response.FailureCount}");
        }

        public async Task SendNotificationAsync(Guid userId, string title, string body, string type, string? data = null)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Create a notification record in the database
                var notification = new MeGo.Api.Models.Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = body,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                // If user has FCM token, send push notification
                // This would require adding FcmToken to User model
                // For now, just save to database
                Console.WriteLine($"✅ Notification saved for user {userId}: {title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending notification: {ex.Message}");
            }
        }
    }
}
