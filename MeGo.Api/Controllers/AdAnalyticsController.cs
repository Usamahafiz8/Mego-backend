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
    public class AdAnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdAnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        // Track ad view
        [HttpPost("ad/{adId}/view")]
        public async Task<IActionResult> TrackView(int adId)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.ViewCount++;
            
            var analytics = await _context.AdAnalytics
                .FirstOrDefaultAsync(a => a.AdId == adId);

            if (analytics == null)
            {
                analytics = new AdAnalytics
                {
                    AdId = adId,
                    Views = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AdAnalytics.Add(analytics);
            }
            else
            {
                analytics.Views++;
                analytics.LastViewedAt = DateTime.UtcNow;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { views = analytics.Views });
        }

        // Track ad click
        [HttpPost("ad/{adId}/click")]
        public async Task<IActionResult> TrackClick(int adId)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.ClickCount++;

            var analytics = await _context.AdAnalytics
                .FirstOrDefaultAsync(a => a.AdId == adId);

            if (analytics == null)
            {
                analytics = new AdAnalytics { AdId = adId, Clicks = 1 };
                _context.AdAnalytics.Add(analytics);
            }
            else
            {
                analytics.Clicks++;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { clicks = analytics.Clicks });
        }

        // Track ad save
        [HttpPost("ad/{adId}/save")]
        [Authorize]
        public async Task<IActionResult> TrackSave(int adId)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.SaveCount++;

            var analytics = await _context.AdAnalytics
                .FirstOrDefaultAsync(a => a.AdId == adId);

            if (analytics == null)
            {
                analytics = new AdAnalytics { AdId = adId, Saves = 1 };
                _context.AdAnalytics.Add(analytics);
            }
            else
            {
                analytics.Saves++;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { saves = analytics.Saves });
        }

        // Track ad share
        [HttpPost("ad/{adId}/share")]
        public async Task<IActionResult> TrackShare(int adId)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            ad.ShareCount++;

            var analytics = await _context.AdAnalytics
                .FirstOrDefaultAsync(a => a.AdId == adId);

            if (analytics == null)
            {
                analytics = new AdAnalytics { AdId = adId, Shares = 1 };
                _context.AdAnalytics.Add(analytics);
            }
            else
            {
                analytics.Shares++;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { shares = analytics.Shares });
        }

        // Get analytics for an ad (owner only)
        [HttpGet("ad/{adId}")]
        [Authorize]
        public async Task<IActionResult> GetAnalytics(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            if (ad.UserId != Guid.Parse(userIdStr)) return Forbid();

            var analytics = await _context.AdAnalytics
                .FirstOrDefaultAsync(a => a.AdId == adId);

            if (analytics == null)
            {
                analytics = new AdAnalytics { AdId = adId };
                _context.AdAnalytics.Add(analytics);
                await _context.SaveChangesAsync();
            }

            return Ok(analytics);
        }
    }
}

