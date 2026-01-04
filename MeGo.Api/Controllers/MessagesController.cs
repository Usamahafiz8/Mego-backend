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
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get all messages of a conversation
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(Guid conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.User1Id == guid || c.User2Id == guid));

            if (conversation == null) return Forbid();

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Content,
                    createdAt = m.CreatedAt,
                    fileUrl = m.FileUrl,
                    messageType = m.MessageType,
                    senderId = m.Sender.Id,
                    senderName = m.Sender.Name
                })
                .ToListAsync();

            return Ok(messages);
        }

        // ✅ Send text message
        [HttpPost("{conversationId}/text")]
        public async Task<IActionResult> SendTextMessage(Guid conversationId, [FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.User1Id == guid || c.User2Id == guid));

            if (conversation == null) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Content required");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = guid,
                Content = dto.Content,
                MessageType = "text",
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = message.Id,
                content = message.Content,
                createdAt = message.CreatedAt,
                fileUrl = (string?)null,
                messageType = "text",
                senderId = message.SenderId,
                senderName = (await _context.Users.FindAsync(message.SenderId))?.Name ?? ""
            });
        }

        // ✅ Send image or voice file (FIXED for Swagger)
        [HttpPost("{conversationId}/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendMessageWithFile(
            Guid conversationId,
            [FromForm] SendFileMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.User1Id == guid || c.User2Id == guid));

            if (conversation == null) return Forbid();

            string? fileUrl = null;
            string messageType = "text";

            if (dto.File != null && dto.File.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.File.FileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                fileUrl = "/uploads/" + fileName;

                if (dto.File.ContentType.StartsWith("audio"))
                    messageType = "voice";
                else if (dto.File.ContentType.StartsWith("image"))
                    messageType = "image";
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = guid,
                Content = dto.Content ?? "",
                FileUrl = fileUrl,
                MessageType = messageType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = message.Id,
                content = message.Content,
                createdAt = message.CreatedAt,
                fileUrl,
                messageType,
                senderId = message.SenderId,
                senderName = (await _context.Users.FindAsync(message.SenderId))?.Name ?? ""
            });
        }

        // ✅ Mark message as read
        [HttpPost("{conversationId}/read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(Guid conversationId, Guid messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var message = await _context.Messages
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId);

            if (message == null) return NotFound();

            if (message.SenderId == guid)
                return BadRequest("You cannot mark your own message as read");

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message.Id, message.IsRead });
        }

        // ✅ Delete message
        [HttpDelete("{conversationId}/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid conversationId, Guid messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var message = await _context.Messages
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId);

            if (message == null) return NotFound();
            if (message.SenderId != guid) return Forbid();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { deleted = true, messageId });
        }

        // ✅ Delete conversation
        [HttpDelete("conversation/{conversationId}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var guid = Guid.Parse(userId);

            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.User1Id == guid || c.User2Id == guid));

            if (conversation == null) return NotFound();

            _context.Messages.RemoveRange(conversation.Messages);
            _context.Conversations.Remove(conversation);

            await _context.SaveChangesAsync();

            return Ok(new { deleted = true, conversationId });
        }

        // ✅ DTOs
        public class SendMessageDto
        {
            public string Content { get; set; } = "";
        }

        public class SendFileMessageDto
        {
            public string? Content { get; set; }
            public IFormFile? File { get; set; }
        }
    }
}
