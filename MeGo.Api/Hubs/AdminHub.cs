using Microsoft.AspNetCore.SignalR;

namespace MeGo.Api.Hubs
{
    public class AdminHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"✅ Admin connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"❌ Admin disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        // ✅ Broadcast when a new report is added
        public async Task BroadcastNewReport(object payload)
        {
            await Clients.All.SendAsync("NewReportAdded", payload);
        }

        // ✅ Broadcast when a listing status changes (activate/deactivate)
        public async Task BroadcastListingStatusChanged(object payload)
        {
            await Clients.All.SendAsync("ListingStatusChanged", payload);
        }

        // ✅ NEW: Broadcast when an admin notification is created
        public async Task BroadcastAdminNotification(object payload)
        {
            await Clients.All.SendAsync("NewAdminNotification", payload);
            Console.WriteLine("📢 Admin notification broadcasted.");
        }

        // ✅ NEW: Broadcast when a notification is deleted or updated (optional)
        public async Task BroadcastNotificationUpdate(object payload)
        {
            await Clients.All.SendAsync("AdminNotificationUpdated", payload);
            Console.WriteLine("🔄 Admin notification updated broadcasted.");
        }
    }
}
