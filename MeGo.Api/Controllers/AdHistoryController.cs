using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;
using System.Text.Json;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    [Authorize]
    public class AdHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdHistoryController(AppDbContext context)
        {
            _context = context;
        }

        // Get ad history
        [HttpGet("ad/{adId}")]
        public async Task<IActionResult> GetAdHistory(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();
            if (ad.UserId != userId) return Forbid();

            var history = await _context.AdHistories
                .Where(h => h.AdId == adId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return Ok(history);
        }

        // Helper method to record ad history (called from AdsController)
        public static async Task RecordAdHistory(AppDbContext context, int adId, string action, Ad? previousAd = null, Ad? newAd = null)
        {
            string? previousValue = null;
            string? newValue = null;

            if (previousAd != null)
            {
                previousValue = JsonSerializer.Serialize(new
                {
                    previousAd.Title,
                    previousAd.Description,
                    previousAd.Price,
                    previousAd.Status
                });
            }

            if (newAd != null)
            {
                newValue = JsonSerializer.Serialize(new
                {
                    newAd.Title,
                    newAd.Description,
                    newAd.Price,
                    newAd.Status
                });
            }

            var history = new AdHistory
            {
                AdId = adId,
                Action = action,
                PreviousValue = previousValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            context.AdHistories.Add(history);
            await context.SaveChangesAsync();
        }
    }
}

