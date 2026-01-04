using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConversationsController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Get all conversations of logged-in user
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var guid = Guid.Parse(userId);

        var conversations = await _context.Conversations
            .Where(c => c.User1Id == guid || c.User2Id == guid)
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .Select(c => new
            {
                c.Id,
                User1 = new { c.User1.Id, c.User1.Name },
                User2 = new { c.User2.Id, c.User2.Name },
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        m.Content,
                        m.CreatedAt,
                        Sender = new { m.Sender.Id, m.Sender.Name }
                    })
                    .FirstOrDefault()
            })
            .OrderByDescending(c => c.LastMessage.CreatedAt)
            .ToListAsync();

        return Ok(conversations);
    }

    // ✅ Start a new conversation
    [HttpPost("{receiverId}")]
    public async Task<IActionResult> StartConversation(Guid receiverId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var guid = Guid.Parse(userId);

        var existing = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                (c.User1Id == guid && c.User2Id == receiverId) ||
                (c.User1Id == receiverId && c.User2Id == guid));

        if (existing != null)
            return Ok(existing);

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = guid,
            User2Id = receiverId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return Ok(conversation);
    }
}


