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
    public class LoyaltyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoyaltyController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Helper to get logged-in user ID
        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --------------------------------------------------------------------
        // 🎡 1. SPIN WHEEL API
        // --------------------------------------------------------------------
        [HttpPost("spin")]
        public async Task<IActionResult> SpinWheel()
        {
            var userId = GetUserId();

            // Check if user already spun today
            var today = DateTime.UtcNow.Date;
            var lastSpin = await _context.SpinHistory
                .Where(s => s.UserId == userId && s.SpinDate.Date == today)
                .FirstOrDefaultAsync();

            if (lastSpin != null)
                return BadRequest(new { message = "You already used your spin today!" });

            // Random prizes
            var prizes = new List<(string type, int value)>
            {
                ("Points", 10),
                ("Points", 25),
                ("Points", 50),
                ("Coin", 1),
                ("Boost", 1),
                ("Voucher", 0)
            };

            var random = new Random();
            var prize = prizes[random.Next(prizes.Count)];

            var history = new SpinHistory
            {
                UserId = userId,
                PrizeType = prize.type,
                PrizeValue = prize.value,
                SpinDate = DateTime.UtcNow
            };

            _context.SpinHistory.Add(history);

            // Update points if Points prize
            var points = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);
            if (points == null)
            {
                points = new UserPoints { UserId = userId };
                _context.UserPoints.Add(points);
            }

            if (prize.type == "Points")
            {
                points.TotalPoints += prize.value;
                points.AvailablePoints += prize.value;
                points.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"You won {prize.value} {prize.type}!",
                prize = prize.type,
                value = prize.value
            });
        }

        // --------------------------------------------------------------------
        // 📅 2. TASK SYSTEM (Daily/Weekly)
        // --------------------------------------------------------------------
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var completedToday = await _context.TaskHistory
                .Where(t => t.UserId == userId && t.CompletedAt.Date == today)
                .ToListAsync();

            var allTasks = new List<LoyaltyTaskDefinition>
            {
                new("dailyLogin", "Daily Login Bonus", "Open the app each day", 10, "daily"),
                new("postAd", "Post a New Ad", "Publish at least one listing", 25, "daily"),
                new("shareAd", "Share Any Ad", "Share your listing with friends", 15, "daily"),
                new("referFriend", "Invite A Friend", "Send your referral code", 40, "weekly"),
                new("completeProfile", "Complete Profile", "Verify and update profile info", 30, "once")
            };

            var response = allTasks.Select(task => new
            {
                task.TaskType,
                task.Title,
                task.Description,
                task.Points,
                task.Frequency,
                completed = completedToday.Any(c => c.TaskType == task.TaskType)
            });

            return Ok(response);
        }

        [HttpPost("complete-task")]
        public async Task<IActionResult> CompleteTask([FromBody] TaskDto dto)
        {
            var userId = GetUserId();

            // Check if task already completed today
            var today = DateTime.UtcNow.Date;
            var exists = await _context.TaskHistory
                .AnyAsync(t => t.UserId == userId && t.TaskType == dto.TaskType && t.CompletedAt.Date == today);

            if (exists)
                return BadRequest(new { message = "Task already completed today." });

            // Add to history
            var history = new TaskHistory
            {
                UserId = userId,
                TaskType = dto.TaskType,
                PointsEarned = dto.Points,
                CompletedAt = DateTime.UtcNow
            };

            _context.TaskHistory.Add(history);

            // Update user points
            var points = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);
            if (points == null)
            {
                points = new UserPoints { UserId = userId };
                _context.UserPoints.Add(points);
            }

            points.TotalPoints += dto.Points;
            points.AvailablePoints += dto.Points;
            points.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Task '{dto.TaskType}' completed! You earned {dto.Points} points.",
                totalPoints = points.TotalPoints
            });
        }

        // --------------------------------------------------------------------
        // 👥 3. REFERRAL SYSTEM
        // --------------------------------------------------------------------
        [HttpPost("generate-referral")]
        public async Task<IActionResult> GenerateReferral()
        {
            var userId = GetUserId();

            var code = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var referral = new Referral
            {
                ReferrerId = userId,
                ReferralCode = code
            };

            _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();

            return Ok(new { referralCode = code });
        }

        [HttpPost("redeem-referral")]
        public async Task<IActionResult> RedeemReferral([FromBody] RedeemReferralDto dto)
        {
            var userId = GetUserId();
            var referral = await _context.Referrals.FirstOrDefaultAsync(r => r.ReferralCode == dto.ReferralCode);

            if (referral == null)
                return BadRequest(new { message = "Invalid referral code." });

            if (referral.ReferrerId == userId)
                return BadRequest(new { message = "You cannot use your own code." });

            if (referral.RewardGiven)
                return BadRequest(new { message = "Referral already used." });

            referral.RewardGiven = true;
            referral.ReferredUserId = userId;

            // Reward both users
            var referrerPoints = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == referral.ReferrerId);
            var referredPoints = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);

            if (referrerPoints == null)
            {
                referrerPoints = new UserPoints { UserId = referral.ReferrerId };
                _context.UserPoints.Add(referrerPoints);
            }

            if (referredPoints == null)
            {
                referredPoints = new UserPoints { UserId = userId };
                _context.UserPoints.Add(referredPoints);
            }

            referrerPoints.TotalPoints += 50;
            referrerPoints.AvailablePoints += 50;

            referredPoints.TotalPoints += 25;
            referredPoints.AvailablePoints += 25;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Referral successful! Referrer +50 points, you +25 points.",
                referrerId = referral.ReferrerId
            });
        }

        // GET Referral Code
        [HttpGet("referral")]
        public async Task<IActionResult> GetReferralCode()
        {
            var userId = GetUserId();
            var referral = await _context.Referrals
                .Where(r => r.ReferrerId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (referral == null)
            {
                // Auto-generate if doesn't exist
                var code = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                referral = new Referral
                {
                    ReferrerId = userId,
                    ReferralCode = code
                };
                _context.Referrals.Add(referral);
                await _context.SaveChangesAsync();
            }

            return Ok(new { code = referral.ReferralCode });
        }

        // GET Referral Stats
        [HttpGet("referral/stats")]
        public async Task<IActionResult> GetReferralStats()
        {
            var userId = GetUserId();
            
            var totalReferrals = await _context.Referrals
                .CountAsync(r => r.ReferrerId == userId && r.RewardGiven);

            var earnedPoints = await _context.TaskHistory
                .Where(t => t.UserId == userId && t.TaskType == "referFriend")
                .SumAsync(t => (int?)t.PointsEarned) ?? 0;

            // Count pending referrals (reward not given yet but referral exists)
            var pendingReferrals = await _context.Referrals
                .CountAsync(r => r.ReferrerId == userId && !r.RewardGiven && r.ReferredUserId != null);

            return Ok(new
            {
                total = totalReferrals,
                earned = earnedPoints,
                pending = pendingReferrals
            });
        }

        // --------------------------------------------------------------------
        // 💰 4. Get My Points Summary
        // --------------------------------------------------------------------
        [HttpGet("points")]
        public async Task<IActionResult> GetPoints()
        {
            var userId = GetUserId();
            var points = await _context.UserPoints.FirstOrDefaultAsync(p => p.UserId == userId);
            if (points == null)
                return Ok(new { total = 0, available = 0 });

            return Ok(new
            {
                total = points.TotalPoints,
                available = points.AvailablePoints,
                lastUpdated = points.LastUpdated
            });
        }
    }

    // ------------------ DTOs ------------------
    public record LoyaltyTaskDefinition(string TaskType, string Title, string Description, int Points, string Frequency);

    public class TaskDto
    {
        public string TaskType { get; set; } = "";
        public int Points { get; set; } = 10;
    }

    public class RedeemReferralDto
    {
        public string ReferralCode { get; set; } = "";
    }
}
