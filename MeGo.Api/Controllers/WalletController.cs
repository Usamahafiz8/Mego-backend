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
    public class WalletController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WalletController(AppDbContext context) { _context = context; }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // ✅ Get Wallet Summary (points, balance, etc.)
        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var userId = GetUserId();
            var userPoints = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);
            var user = await _context.Users.FindAsync(userId);

            if (userPoints == null)
            {
                userPoints = new UserPoints { UserId = userId };
                _context.UserPoints.Add(userPoints);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                totalPoints = userPoints.TotalPoints,
                availablePoints = userPoints.AvailablePoints,
                coinsBalance = user?.CoinsBalance ?? 0,
                lastUpdated = userPoints.LastUpdated
            });
        }

        // ✅ Withdraw Request
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto dto)
        {
            var userId = GetUserId();
            var userPoints = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (userPoints == null)
            {
                userPoints = new UserPoints { UserId = userId };
                _context.UserPoints.Add(userPoints);
                await _context.SaveChangesAsync();
            }

            // Convert amount to points if Points not provided
            // Rate: 1 Point = PKR 0.10, so Points = Amount / 0.10 = Amount * 10
            int pointsToUse = dto.Points;
            if (pointsToUse == 0 && dto.Amount > 0)
            {
                pointsToUse = (int)(dto.Amount * 10); // Convert PKR to points
            }

            // Minimum withdrawal check
            const int minPoints = 500;
            if (pointsToUse < minPoints)
                return BadRequest(new { message = $"Minimum withdrawal is {minPoints} points (PKR {minPoints * 0.1m:F2})." });

            if (userPoints.AvailablePoints < pointsToUse)
                return BadRequest(new { message = "Not enough points available." });

            // Deduct immediately
            userPoints.AvailablePoints -= pointsToUse;
            userPoints.LastUpdated = DateTime.UtcNow;

            // Ensure amount is set
            decimal amount = dto.Amount > 0 ? dto.Amount : (pointsToUse * 0.1m);

            var transaction = new WalletTransaction
            {
                UserId = userId,
                Method = dto.Method,
                Amount = amount,
                PointsUsed = pointsToUse
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Withdrawal request created successfully!" });
        }

        // ✅ Get My Transactions
        [HttpGet("transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userId = GetUserId();
            var txns = await _context.WalletTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(txns);
        }

        // ✅ Get Recent Withdrawals
        [HttpGet("recent-withdrawals")]
        public async Task<IActionResult> GetRecentWithdrawals([FromQuery] int limit = 10)
        {
            var userId = GetUserId();
            var txns = await _context.WalletTransactions
                .Where(t => t.UserId == userId && t.Method != null)
                .OrderByDescending(t => t.CreatedAt)
                .Take(limit)
                .Select(t => new
                {
                    id = t.Id,
                    method = t.Method,
                    amount = t.Amount,
                    pointsUsed = t.PointsUsed,
                    status = t.Status ?? "Pending",
                    createdAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(new { withdrawals = txns });
        }
    }

    public class WithdrawDto
    {
        public string Method { get; set; } = "";
        public decimal Amount { get; set; }
        public int Points { get; set; } = 0; // Optional, will be calculated from Amount if not provided
    }
}
