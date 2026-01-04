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
    public class SellerRatingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SellerRatingController(AppDbContext context)
        {
            _context = context;
        }

        // Rate seller after chat
        [HttpPost]
        public async Task<IActionResult> RateSeller([FromBody] RateSellerDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var raterId = Guid.Parse(userIdStr);

            // Verify conversation exists and user was part of it
            if (dto.ConversationId.HasValue)
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == dto.ConversationId.Value &&
                        (c.User1Id == raterId || c.User2Id == raterId));

                if (conversation == null)
                    return NotFound("Conversation not found or you weren't part of it");

                // Ensure user is rating the other person
                var sellerId = conversation.User1Id == raterId ? conversation.User2Id : conversation.User1Id;
                if (dto.SellerId != sellerId)
                    return BadRequest("You can only rate the other person in the conversation");
            }

            // Check if already rated
            var existing = await _context.SellerRatings
                .FirstOrDefaultAsync(r => r.RaterId == raterId && r.SellerId == dto.SellerId &&
                    (dto.ConversationId == null || r.ConversationId == dto.ConversationId));

            if (existing != null)
                return BadRequest("You have already rated this seller");

            var rating = new SellerRating
            {
                RaterId = raterId,
                SellerId = dto.SellerId,
                ConversationId = dto.ConversationId,
                AdId = dto.AdId,
                Rating = dto.Rating,
                Review = dto.Review,
                CreatedAt = DateTime.UtcNow
            };

            _context.SellerRatings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(rating);
        }

        // Get seller ratings
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetSellerRatings(Guid sellerId)
        {
            var ratings = await _context.SellerRatings
                .Where(r => r.SellerId == sellerId)
                .Include(r => r.Rater)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Review,
                    r.CreatedAt,
                    rater = new { r.Rater.Id, r.Rater.Name, r.Rater.ProfileImage }
                })
                .ToListAsync();

            var averageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0;
            var totalRatings = ratings.Count;

            return Ok(new
            {
                averageRating = Math.Round(averageRating, 1),
                totalRatings,
                ratings
            });
        }

        // Get my ratings (as a seller)
        [HttpGet("my-ratings")]
        public async Task<IActionResult> GetMyRatings()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ratings = await _context.SellerRatings
                .Where(r => r.SellerId == userId)
                .Include(r => r.Rater)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ratings);
        }
    }

    public class RateSellerDto
    {
        public Guid SellerId { get; set; }
        public Guid? ConversationId { get; set; }
        public int? AdId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Review { get; set; }
    }
}

