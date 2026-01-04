// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using MeGo.Api.Data;
// using MeGo.Api.Models;
// using MeGo.Api.Services;
// using System.Security.Claims;

// namespace MeGo.Api.Controllers;

// [ApiController]
// [Route("v1/[controller]")]
// [Authorize]
// public class NotificationsController : ControllerBase
// {
//     private readonly AppDbContext _context;
//     private readonly NotificationService _notifier;

//     public NotificationsController(AppDbContext context, NotificationService notifier)
//     {
//         _context = context;
//         _notifier = notifier;
//     }

//     // ✅ Save/Update device token
//     [HttpPost("register")]
//     public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenDto dto)
//     {
//         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         if (string.IsNullOrEmpty(userId)) return Unauthorized();
//         var guid = Guid.Parse(userId);

//         // check if already exists
//         var existing = await _context.DeviceTokens
//             .FirstOrDefaultAsync(t => t.UserId == guid && t.Token == dto.Token);

//         if (existing == null)
//         {
//             var token = new DeviceToken
//             {
//                 Id = Guid.NewGuid(),
//                 UserId = guid,
//                 Token = dto.Token,
//                 Platform = dto.Platform ?? "unknown",
//                 IsActive = true,
//                 CreatedAt = DateTime.UtcNow
//             };
//             _context.DeviceTokens.Add(token);
//         }
//         else
//         {
//             existing.IsActive = true;
//             existing.Platform = dto.Platform ?? existing.Platform;
//         }

//         await _context.SaveChangesAsync();
//         return Ok(new { message = "Token registered" });
//     }

//     // ✅ Test endpoint: send push to myself
//     [HttpPost("test")]
//     public async Task<IActionResult> TestNotification([FromBody] TestDto dto)
//     {
//         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         if (string.IsNullOrEmpty(userId)) return Unauthorized();
//         var guid = Guid.Parse(userId);

//         var tokens = await _context.DeviceTokens
//             .Where(t => t.UserId == guid && t.IsActive)
//             .Select(t => t.Token)
//             .ToListAsync();

//         if (!tokens.Any()) return BadRequest(new { message = "No active tokens" });

//         await _notifier.SendMulticastAsync(tokens, dto.Title, dto.Body);

//         return Ok(new { message = "Notification sent" });
//     }
// }

// // ✅ DTOs
// public class RegisterTokenDto
// {
//     public string Token { get; set; } = "";
//     public string? Platform { get; set; }
// }
// public class TestDto
// {
//     public string Title { get; set; } = "Test";
//     public string Body { get; set; } = "Hello from server!";
// }
using MeGo.Api.Data;
using MeGo.Api.Hubs;
using MeGo.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<AdminHub> _hub;

        public NotificationsController(AppDbContext db, IHubContext<AdminHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // GET v1/notifications     -> user's notifications (auth required)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var notifications = await _db.Notifications
                .Where(n => n.UserId == null || n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // POST v1/notifications/read/{id}  -> mark single as read
        [HttpPost("read/{id}")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = Guid.Parse(userIdStr);

            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            // ensure user only marks their own or global notifications
            if (notification.UserId != null && notification.UserId != userId)
                return Forbid();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // DELETE v1/notifications/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = Guid.Parse(userIdStr);

            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();
            if (notification.UserId != null && notification.UserId != userId) return Forbid();

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
