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
    public class BoostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BoostController(AppDbContext context)
        {
            _context = context;
        }

        // Generate share link for instant boost via referrals
        [HttpPost("ad/{adId}/share-link")]
        public async Task<IActionResult> GenerateShareLink(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();
            if (ad.UserId != userId) return Forbid();

            // Check if boost referral already exists
            var existing = await _context.BoostReferrals
                .FirstOrDefaultAsync(b => b.AdId == adId && b.UserId == userId && !b.BoostEarned);

            if (existing != null)
            {
                return Ok(new { shareLink = existing.ShareLink, clickCount = existing.ClickCount });
            }

            var shareLink = $"https://mego.com.pk/ad/{adId}?ref={Guid.NewGuid().ToString("N").Substring(0, 8)}";

            var boostReferral = new BoostReferral
            {
                AdId = adId,
                UserId = userId,
                ShareLink = shareLink,
                ClickCount = 0,
                RequiredClicks = 3,
                BoostEarned = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.BoostReferrals.Add(boostReferral);
            await _context.SaveChangesAsync();

            return Ok(new { shareLink, clickCount = 0 });
        }

        // Track share link click
        [HttpPost("track-click")]
        public async Task<IActionResult> TrackShareClick([FromQuery] string refCode, [FromQuery] int adId)
        {
            var boostReferral = await _context.BoostReferrals
                .FirstOrDefaultAsync(b => b.AdId == adId && b.ShareLink.Contains(refCode));

            if (boostReferral == null)
                return NotFound("Invalid referral link");

            if (boostReferral.BoostEarned)
                return Ok(new { message = "Boost already earned", clickCount = boostReferral.ClickCount });

            boostReferral.ClickCount++;

            // Check if threshold reached
            if (boostReferral.ClickCount >= boostReferral.RequiredClicks)
            {
                boostReferral.BoostEarned = true;
                boostReferral.BoostEarnedAt = DateTime.UtcNow;

                // Boost the ad
                var ad = await _context.Ads.FindAsync(adId);
                if (ad != null)
                {
                    ad.IsBoosted = true;
                    ad.BoostedUntil = DateTime.UtcNow.AddDays(7);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                clickCount = boostReferral.ClickCount,
                requiredClicks = boostReferral.RequiredClicks,
                boostEarned = boostReferral.BoostEarned
            });
        }

        // Get boost status for an ad
        [HttpGet("ad/{adId}/status")]
        public async Task<IActionResult> GetBoostStatus(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();
            if (ad.UserId != userId) return Forbid();

            var boostReferral = await _context.BoostReferrals
                .FirstOrDefaultAsync(b => b.AdId == adId && b.UserId == userId);

            if (boostReferral == null)
            {
                return Ok(new
                {
                    isBoosted = ad.IsBoosted,
                    boostedUntil = ad.BoostedUntil,
                    hasShareLink = false
                });
            }

            return Ok(new
            {
                isBoosted = ad.IsBoosted,
                boostedUntil = ad.BoostedUntil,
                hasShareLink = true,
                shareLink = boostReferral.ShareLink,
                clickCount = boostReferral.ClickCount,
                requiredClicks = boostReferral.RequiredClicks,
                boostEarned = boostReferral.BoostEarned
            });
        }
    }
}

