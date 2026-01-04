using Microsoft.AspNetCore.Mvc;
using MeGo.Api.Data;
using MeGo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Controllers.UsersArea
{
    [ApiController]
    [Route("v1/[controller]")] 
    public class UsersNotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersNotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification deleted" });
        }
    }
}
