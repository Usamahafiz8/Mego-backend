using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stats")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var usersCount = await _context.Users.CountAsync();
            var verifiedUsers = await _context.Users.CountAsync(u => u.EmailConfirmed);
            var bannedUsers = await _context.Users.CountAsync(u => u.Status == "banned");

            var activeAds = await _context.Ads.CountAsync(a => a.Status == "approved" || a.Status == "active");
            var pendingAds = await _context.Ads.CountAsync(a => a.Status == "pending");
            var rejectedAds = await _context.Ads.CountAsync(a => a.Status == "rejected");
            var soldAds = await _context.Ads.CountAsync(a => a.Status == "sold");

            var totalReports = await _context.Reports.CountAsync();
            var reportsToday = await _context.Reports.CountAsync(r => r.CreatedAt.Date == DateTime.UtcNow.Date);

            // Latest 5 Users
            var latestUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Status,
                    u.CreatedAt
                })
                .ToListAsync();

            // Latest 5 Listings
            var latestAds = await _context.Ads
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Status,
                    a.CreatedAt,
                    a.Price
                })
                .ToListAsync();

            // Latest 5 Reports
            var latestReports = await _context.Reports
                .Include(r => r.Ad)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new
                {
                    r.Id,
                    r.Reason,
                    r.Status,
                    ListingTitle = r.Ad.Title,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                usersCount,
                verifiedUsers,
                bannedUsers,

                activeAds,
                pendingAds,
                rejectedAds,
                soldAds,

                totalReports,
                reportsToday,

                latestUsers,
                latestAds,
                latestReports
            });
        }
    }
}
