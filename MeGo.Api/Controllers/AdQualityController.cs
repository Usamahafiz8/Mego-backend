using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using MeGo.Api.Services;
using System.Security.Claims;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AdQualityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AdQualityScoreService _qualityService;

        public AdQualityController(AppDbContext context, AdQualityScoreService qualityService)
        {
            _context = context;
            _qualityService = qualityService;
        }

        // Get quality score for an ad
        [HttpGet("ad/{adId}")]
        public async Task<IActionResult> GetQualityScore(int adId)
        {
            var score = await _context.AdQualityScores
                .FirstOrDefaultAsync(s => s.AdId == adId);

            if (score == null)
            {
                // Calculate if doesn't exist
                await _qualityService.CalculateAndSaveQualityScore(adId);
                score = await _context.AdQualityScores
                    .FirstOrDefaultAsync(s => s.AdId == adId);
            }

            return Ok(score);
        }

        // Recalculate quality score
        [HttpPost("ad/{adId}/recalculate")]
        [Authorize]
        public async Task<IActionResult> RecalculateScore(int adId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return NotFound();

            if (ad.UserId != Guid.Parse(userIdStr)) return Forbid();

            await _qualityService.CalculateAndSaveQualityScore(adId);
            var score = await _context.AdQualityScores
                .FirstOrDefaultAsync(s => s.AdId == adId);

            return Ok(score);
        }
    }
}

