using MeGo.Api.Data;
using MeGo.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<AdminHub> _hub;

        public AdminReportsController(AppDbContext db, IHubContext<AdminHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ✅ Get all reports
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _db.Reports
                .Include(r => r.Ad)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = reports.Select(r => new
            {
                id = r.Id,
                reason = r.Reason,
                status = r.Status,
                userId = r.UserId,
                adId = r.AdId,
                userName = r.User?.Name ?? "Unknown",
                listingTitle = r.Ad?.Title ?? "Unknown",
                userIsBanned = r.User?.IsBanned ?? false,
                listingStatus = r.Ad?.Status ?? "unknown"
            });

            return Ok(new { reports = result });
        }

        // ✅ Resolve report
        [HttpPost("{id}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = "resolved";
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ReportStatusUpdated", new
            {
                report.Id,
                report.Status
            });

            return Ok(new { success = true, message = "Report resolved successfully." });
        }

        // ✅ Ban user
        [HttpPost("user/{userId}/ban")]
        public async Task<IActionResult> BanUser(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = false;
            user.IsBanned = true;
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("UserStatusUpdated", new
            {
                user.Id,
                user.IsActive,
                user.IsBanned
            });

            return Ok(new { success = true, message = "User banned successfully." });
        }

        // ✅ Unban user
        [HttpPost("user/{userId}/unban")]
        public async Task<IActionResult> UnbanUser(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = true;
            user.IsBanned = false;
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("UserStatusUpdated", new
            {
                user.Id,
                user.IsActive,
                user.IsBanned
            });

            return Ok(new { success = true, message = "User reactivated successfully." });
        }

        // ✅ Deactivate listing
        [HttpPost("listing/{adId}/deactivate")]
        public async Task<IActionResult> DeactivateListing(int adId)
        {
            var ad = await _db.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.Status = "deactivated";
            ad.IsActive = false;
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ListingStatusChanged", new
            {
                ad.Id,
                ad.Status,
                ad.IsActive
            });

            return Ok(new { success = true, message = "Listing deactivated successfully." });
        }

        // ✅ Reactivate listing
        [HttpPost("listing/{adId}/reactivate")]
        public async Task<IActionResult> ReactivateListing(int adId)
        {
            var ad = await _db.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.Status = "active";
            ad.IsActive = true;
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ListingStatusChanged", new
            {
                ad.Id,
                ad.Status,
                ad.IsActive
            });

            return Ok(new { success = true, message = "Listing reactivated successfully." });
        }

        // ✅ Delete report
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound();

            _db.Reports.Remove(report);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ReportDeleted", new { id });

            return Ok(new { success = true, message = "Report deleted successfully." });
        }
    }
}
