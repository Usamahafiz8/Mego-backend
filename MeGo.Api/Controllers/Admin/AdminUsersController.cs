using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminUsersController(AppDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------
        // ✅ GET ALL USERS (search + filter)
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? search, [FromQuery] string? filter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Phone.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search))
                );
            }

            if (filter == "verified")
                query = query.Where(u => u.EmailConfirmed == true);

            if (filter == "banned")
                query = query.Where(u => u.Status == "banned");

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.Status,
                    u.EmailConfirmed,
                    u.ProfileImage,

                    // NEW
                    AdsCount = _context.Ads.Count(a => a.UserId == u.Id),
                    ReportCount = _context.Reports.Count(r => r.UserId == u.Id)
                })
                .ToListAsync();

            return Ok(new { users });
        }

        // -------------------------------------------------------------
        // ✅ GET SINGLE USER DETAILS + ADS + REPORTS + NOTIFICATIONS
        // -------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.KycInfo)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Fetch Ads
            var ads = await _context.Ads
                .Where(a => a.UserId == id)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Price,
                    a.Status,
                    a.Category,
                    a.CreatedAt,
                    a.RejectedReason,
                    Media = a.Media.Select(m => new
                    {
                        m.FilePath,
                        m.MediaType
                    })
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Fetch Reports
            var reports = await _context.Reports
                .Where(r => r.UserId == id)
                .Select(r => new
                {
                    r.Id,
                    r.Reason,
                    r.Status,
                    r.CreatedAt,
                    ListingTitle = r.Ad.Title
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Notifications
            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Phone,
                    user.Status,
                    user.CreatedAt,
                    user.EmailConfirmed,
                    user.ProfileImage,
                    user.DarkMode,
                    user.NotificationsEnabled,

                    AdsCount = ads.Count,
                    ReportCount = reports.Count,

                    Kyc = user.KycInfo == null ? null : new
                    {
                        user.KycInfo.CnicNumber,
                        user.KycInfo.CnicFrontImageUrl,
                        user.KycInfo.CnicBackImageUrl,
                        user.KycInfo.SelfieUrl,
                        user.KycInfo.Status
                    }
                },

                ads,
                reports,
                notifications
            });
        }

        // -------------------------------------------------------------
        // ✅ BAN USER
        // -------------------------------------------------------------
        [HttpPost("{id}/ban")]
        public async Task<IActionResult> BanUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Status = "banned";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{user.Name} has been banned." });
        }

        // -------------------------------------------------------------
        // ✅ UNBAN USER
        // -------------------------------------------------------------
        [HttpPost("{id}/unban")]
        public async Task<IActionResult> UnbanUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Status = "active";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{user.Name} has been unbanned." });
        }
    }
}
