using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeGo.Api.Data;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Services;
using System.Security.Claims;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize] // ✅ Only authenticated users can access
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RewardService _rewardService;

    public UsersController(AppDbContext context, RewardService rewardService)
    {
        _context = context;
        _rewardService = rewardService;
    }

    // ✅ Get current logged-in user
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid token");

        if (!Guid.TryParse(userId, out var guid))
            return Unauthorized("Invalid user ID in token");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == guid);

        if (user == null)
            return NotFound("User not found");

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Phone,
            user.Email,
            user.VerificationTier,
            user.CoinsBalance,
            user.CreatedAt
        });
    }

    // ✅ Get user by ID (Public - for seller profiles)
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow public access for seller profiles
    public async Task<IActionResult> GetUserById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return BadRequest(new { message = "Invalid user ID format" });

        var user = await _context.Users
            .Include(u => u.KycInfo)
            .FirstOrDefaultAsync(u => u.Id == guid);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Get user's ads count
        var adsCount = await _context.Ads
            .CountAsync(a => a.UserId == guid && (a.Status == "approved" || a.Status == "active"));

        // Get seller ratings
        var ratings = await _context.SellerRatings
            .Where(r => r.SellerId == guid)
            .ToListAsync();

        var averageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0;
        var totalRatings = ratings.Count;

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            phone = user.Phone,
            profileImage = user.ProfileImage,
            createdAt = user.CreatedAt,
            verificationTier = user.VerificationTier,
            kycStatus = user.KycInfo?.Status,
            adsCount,
            averageRating = Math.Round(averageRating, 1),
            totalRatings
        });
    }

    // ✅ Update profile (+20 coins if completed)
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null) return NotFound();

        user.Name = dto.Name ?? user.Name;
        user.Email = dto.Email ?? user.Email;

        await _context.SaveChangesAsync();

        // ✅ Reward 20 coins if profile is complete
        var isComplete = !string.IsNullOrEmpty(user.Name) &&
                        !string.IsNullOrEmpty(user.Email) &&
                        !string.IsNullOrEmpty(user.Phone) &&
                        user.ProfileImage != null;

        if (isComplete)
        {
            await _rewardService.GiveRewardAsync(user.Id, "completeProfile", 20);
        }

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.ProfileImage
        });
    }
}

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}
