using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using Microsoft.AspNetCore.SignalR;
using MeGo.Api.Hubs;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/listings")]
    public class AdminListingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AdminHub> _hub;

        public AdminListingsController(AppDbContext context, IHubContext<AdminHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // GET api/admin/listings?status=pending|approved|rejected
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.Ads.Include(a => a.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status.ToLower() == status.ToLower());

            var ads = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    price = a.Price,
                    status = a.Status ?? "pending",
                    rejectedReason = a.RejectedReason,
                    createdAt = a.CreatedAt,
                    userName = a.User != null ? a.User.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(new { listings = ads });
        }

        // POST api/admin/listings/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound(new { message = "Ad not found" });

            ad.Status = "approved";
            ad.IsActive = true;
            ad.ApprovedAt = DateTime.UtcNow;
            ad.RejectedAt = null;
            ad.RejectedReason = null;

            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ListingStatusChanged", new
            {
                id = ad.Id,
                status = ad.Status
            });

            return Ok(new { message = "Ad approved" });
        }

        // DTO for reject reason
        public class RejectDto
        {
            public string Reason { get; set; } = "";
        }

        // POST api/admin/listings/{id}/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectDto dto)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound(new { message = "Ad not found" });

            ad.Status = "rejected";
            ad.IsActive = false;
            ad.RejectedAt = DateTime.UtcNow;
            ad.RejectedReason = string.IsNullOrWhiteSpace(dto?.Reason) ? "No reason provided" : dto.Reason;

            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ListingStatusChanged", new
            {
                id = ad.Id,
                status = ad.Status,
                reason = ad.RejectedReason
            });

            return Ok(new { message = "Ad rejected" });
        }

        // DELETE api/admin/listings/{id}
        [HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var ad = await _context.Ads
        .Include(a => a.Media)
        .Include(a => a.Reports)
        .Include(a => a.Favorites)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (ad == null)
        return NotFound();

    // 🗑 DELETE media files
    foreach (var media in ad.Media)
    {
        try
        {
            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                media.FilePath.TrimStart('/')
            );

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
        catch { }
    }

    // 🗑 DELETE all related database entries
    _context.Media.RemoveRange(ad.Media);
    _context.Reports.RemoveRange(ad.Reports);
    _context.Favorites.RemoveRange(ad.Favorites);

    // (Optional) DELETE chats linked to this Ad if your system supports it
    // _context.Messages.RemoveRange(_context.Messages.Where(m => m.AdId == id));
    // _context.Conversations.RemoveRange(_context.Conversations.Where(c => c.AdId == id));

    // 🗑 Finally remove ad
    _context.Ads.Remove(ad);

    await _context.SaveChangesAsync();

    // Notify admin dashboards (SignalR)
    await _hub.Clients.All.SendAsync("ListingDeleted", new { Id = id });

    return Ok(new { message = "Ad deleted successfully" });
}


    }
}
