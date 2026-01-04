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
    public class PointsExchangeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PointsExchangeController(AppDbContext context)
        {
            _context = context;
        }

        // Exchange points for coins
        [HttpPost("points-to-coins")]
        public async Task<IActionResult> ExchangePointsToCoins([FromBody] ExchangePointsDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var userPoints = await _context.UserPoints
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userPoints == null || userPoints.AvailablePoints < dto.Points)
                return BadRequest("Insufficient points");

            // Exchange rate: 10 points = 1 coin
            int coinsToReceive = dto.Points / 10;
            if (coinsToReceive < 1)
                return BadRequest("Minimum 10 points required for 1 coin");

            userPoints.AvailablePoints -= dto.Points;
            userPoints.LastUpdated = DateTime.UtcNow;

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.CoinsBalance += coinsToReceive;
            }

            var exchange = new PointsExchange
            {
                UserId = userId,
                ExchangeType = "coins",
                PointsUsed = dto.Points,
                ValueReceived = coinsToReceive,
                Status = "completed",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _context.PointsExchanges.Add(exchange);
            await _context.SaveChangesAsync();

            return Ok(new { coinsReceived = coinsToReceive, newBalance = user?.CoinsBalance });
        }

        // Exchange points for boost
        [HttpPost("points-to-boost")]
        public async Task<IActionResult> ExchangePointsToBoost([FromBody] ExchangePointsForBoostDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var userPoints = await _context.UserPoints
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Boost costs 50 points
            int pointsRequired = 50;
            if (userPoints == null || userPoints.AvailablePoints < pointsRequired)
                return BadRequest($"Insufficient points. Boost requires {pointsRequired} points");

            var ad = await _context.Ads.FindAsync(dto.AdId);
            if (ad == null) return NotFound("Ad not found");
            if (ad.UserId != userId) return Forbid();

            userPoints.AvailablePoints -= pointsRequired;
            userPoints.LastUpdated = DateTime.UtcNow;

            // Boost ad for 7 days
            ad.IsBoosted = true;
            ad.BoostedUntil = DateTime.UtcNow.AddDays(7);

            var exchange = new PointsExchange
            {
                UserId = userId,
                ExchangeType = "boost",
                PointsUsed = pointsRequired,
                ValueReceived = 1, // 1 boost
                Status = "completed",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _context.PointsExchanges.Add(exchange);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ad boosted successfully", boostedUntil = ad.BoostedUntil });
        }

        // Exchange points for mobile recharge
        [HttpPost("points-to-mobile-recharge")]
        public async Task<IActionResult> ExchangePointsForMobileRecharge([FromBody] MobileRechargeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var userPoints = await _context.UserPoints
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Mobile recharge: 100 points = 50 PKR recharge
            int pointsRequired = 100;
            if (userPoints == null || userPoints.AvailablePoints < pointsRequired)
                return BadRequest($"Insufficient points. Mobile recharge requires {pointsRequired} points");

            userPoints.AvailablePoints -= pointsRequired;
            userPoints.LastUpdated = DateTime.UtcNow;

            var exchange = new PointsExchange
            {
                UserId = userId,
                ExchangeType = "mobile_recharge",
                PointsUsed = pointsRequired,
                ValueReceived = 50, // 50 PKR
                MobileNetwork = dto.Network,
                MobileNumber = dto.MobileNumber,
                Status = "pending", // Will be processed by admin/automated service
                CreatedAt = DateTime.UtcNow
            };

            _context.PointsExchanges.Add(exchange);
            await _context.SaveChangesAsync();

            // TODO: Integrate with mobile recharge API (Jazz, Zong, Telenor)
            // For now, mark as pending for admin processing

            return Ok(new { message = "Mobile recharge request submitted", exchangeId = exchange.Id });
        }

        // Get exchange history
        [HttpGet("history")]
        public async Task<IActionResult> GetExchangeHistory()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var exchanges = await _context.PointsExchanges
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(exchanges);
        }
    }

    public class ExchangePointsDto
    {
        public int Points { get; set; }
    }

    public class ExchangePointsForBoostDto
    {
        public int AdId { get; set; }
    }

    public class MobileRechargeDto
    {
        public string Network { get; set; } = ""; // Jazz, Zong, Telenor
        public string MobileNumber { get; set; } = "";
    }
}

