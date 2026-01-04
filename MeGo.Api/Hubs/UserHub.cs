using Microsoft.AspNetCore.SignalR;

namespace MeGo.Api.Hubs
{
    public class UserHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"✅ User connected: {Context.ConnectionId}");

            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"📌 Added to group: {userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"❌ User disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // 🔥 Send notification to that specific user
        public async Task SendNotificationToUser(string userId, object payload)
        {
            await Clients.Group(userId).SendAsync("ReceiveUserNotification", payload);
        }
    }
}
