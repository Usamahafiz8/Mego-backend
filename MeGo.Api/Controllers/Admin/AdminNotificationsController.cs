using MeGo.Api.Data;
using MeGo.Api.Hubs;
using MeGo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notifications")]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<AdminHub> _hub;

        public AdminNotificationsController(AppDbContext db, IHubContext<AdminHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ✅ Get all notifications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _db.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(new { notifications });
        }

        // ✅ Create a new notification
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Notification model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { message = "Title is required." });

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            _db.Notifications.Add(model);
            await _db.SaveChangesAsync();

            // 🔔 Broadcast new notification to all connected admins
            await _hub.Clients.All.SendAsync("NewAdminNotification", new
            {
                id = model.Id,
                title = model.Title,
                message = model.Message,
                createdAt = model.CreatedAt
            });

            return Ok(new { success = true, message = "Notification created successfully." });
        }

        // ✅ Update notification
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Notification updated)
        {
            var existing = await _db.Notifications.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = updated.Title;
            existing.Message = updated.Message;
            await _db.SaveChangesAsync();

            // 🔄 Broadcast update
            await _hub.Clients.All.SendAsync("AdminNotificationUpdated", new
            {
                id = existing.Id,
                title = existing.Title,
                message = existing.Message
            });

            return Ok(new { success = true, message = "Notification updated successfully." });
        }

        // ✅ Delete notification
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();

            // 🗑️ Broadcast deletion
            await _hub.Clients.All.SendAsync("AdminNotificationDeleted", new { id });

            return Ok(new { success = true, message = "Notification deleted successfully." });
        }
    }
}
