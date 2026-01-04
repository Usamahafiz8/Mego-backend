using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    [Authorize]
    public class RecentlyViewedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecentlyViewedController(AppDbContext context)
        {
            _context = context;
        }

        // Track recently viewed item
        [HttpPost("ad/{adId}")]
        public async Task<IActionResult> TrackView(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            // Check if already viewed recently (within last hour)
            var existing = await _context.RecentlyViewed
                .FirstOrDefaultAsync(r => r.UserId == userId && r.AdId == adId &&
                    r.ViewedAt > DateTime.UtcNow.AddHours(-1));

            if (existing != null)
            {
                existing.ViewedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "View updated" });
            }

            var viewed = new RecentlyViewed
            {
                UserId = userId,
                AdId = adId,
                ViewedAt = DateTime.UtcNow
            };

            _context.RecentlyViewed.Add(viewed);
            await _context.SaveChangesAsync();

            return Ok(new { message = "View tracked" });
        }

        // Get recently viewed items
        [HttpGet]
        public async Task<IActionResult> GetRecentlyViewed([FromQuery] int limit = 20)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var viewed = await _context.RecentlyViewed
                .Where(r => r.UserId == userId)
                .Include(r => r.Ad)
                    .ThenInclude(a => a.Media)
                .OrderByDescending(r => r.ViewedAt)
                .Take(limit)
                .Select(r => new
                {
                    r.Id,
                    r.ViewedAt,
                    ad = new
                    {
                        r.Ad.Id,
                        r.Ad.Title,
                        r.Ad.Price,
                        r.Ad.ImageUrl,
                        r.Ad.Location,
                        media = r.Ad.Media.Select(m => new { m.FilePath, m.MediaType })
                    }
                })
                .ToListAsync();

            return Ok(viewed);
        }

        // Clear recently viewed
        [HttpDelete]
        public async Task<IActionResult> ClearRecentlyViewed()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var items = await _context.RecentlyViewed
                .Where(r => r.UserId == userId)
                .ToListAsync();

            _context.RecentlyViewed.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Recently viewed cleared" });
        }
    }
}

