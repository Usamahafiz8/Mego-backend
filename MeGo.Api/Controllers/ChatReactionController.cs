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
    public class ChatReactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatReactionController(AppDbContext context)
        {
            _context = context;
        }

        // Add reaction to message
        [HttpPost("message/{messageId}")]
        public async Task<IActionResult> AddReaction(Guid messageId, [FromBody] AddReactionDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            // Verify message exists
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return NotFound();

            // Check if user already reacted with this emoji
            var existing = await _context.ChatReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == dto.Emoji);

            if (existing != null)
            {
                // Remove reaction (toggle)
                _context.ChatReactions.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Reaction removed", removed = true });
            }

            var reaction = new ChatReaction
            {
                MessageId = messageId,
                UserId = userId,
                Emoji = dto.Emoji,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatReactions.Add(reaction);
            await _context.SaveChangesAsync();

            return Ok(reaction);
        }

        // Get reactions for a message
        [HttpGet("message/{messageId}")]
        public async Task<IActionResult> GetReactions(Guid messageId)
        {
            var reactions = await _context.ChatReactions
                .Where(r => r.MessageId == messageId)
                .Include(r => r.User)
                .GroupBy(r => r.Emoji)
                .Select(g => new
                {
                    emoji = g.Key,
                    count = g.Count(),
                    users = g.Select(r => new { r.User.Id, r.User.Name })
                })
                .ToListAsync();

            return Ok(reactions);
        }

        // Remove reaction
        [HttpDelete("message/{messageId}/emoji/{emoji}")]
        public async Task<IActionResult> RemoveReaction(Guid messageId, string emoji)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var reaction = await _context.ChatReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (reaction == null) return NotFound();

            _context.ChatReactions.Remove(reaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reaction removed" });
        }
    }

    public class AddReactionDto
    {
        public string Emoji { get; set; } = ""; // 👍, ❤️, 😂, 😮, 😢, 🙏
    }
}

