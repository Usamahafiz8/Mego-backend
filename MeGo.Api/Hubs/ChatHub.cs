using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MeGo.Api.Data;
using MeGo.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MeGo.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _context;

    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Join a conversation group
    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    // ✅ Leave a conversation group
    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    // ✅ Send a message
    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(content)) return;

        var senderGuid = Guid.Parse(userId);

        var conversation = await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderGuid,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // ✅ prepare message DTO
        var msgDto = new
        {
            id = message.Id,
            conversationId,
            senderId = senderGuid,
            content = message.Content,
            createdAt = message.CreatedAt,
            isRead = message.IsRead
        };

        // ✅ broadcast message to both users
        await Clients.Group(conversationId.ToString())
            .SendAsync("ReceiveMessage", msgDto);
    }

    // ✅ Mark message as read
    public async Task MarkAsRead(Guid messageId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var msg = await _context.Messages.FindAsync(messageId);
        if (msg == null) return;

        // Skip if sender is marking their own message
        if (msg.SenderId.ToString() == userId) return;

        msg.IsRead = true;
        await _context.SaveChangesAsync();

        // ✅ Notify all clients in the conversation
        await Clients.Group(msg.ConversationId.ToString())
            .SendAsync("MessageRead", new { messageId = msg.Id });
    }

    // ✅ Delete a single message
    public async Task DeleteMessage(Guid conversationId, Guid messageId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var guid = Guid.Parse(userId);

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId);

        if (message == null || message.SenderId != guid) return;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        // ✅ Notify clients to remove the deleted message
        await Clients.Group(conversationId.ToString())
            .SendAsync("MessageDeleted", messageId);
    }

    // ✅ Delete full conversation
    public async Task DeleteConversation(Guid conversationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var guid = Guid.Parse(userId);

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return;
        if (conversation.User1Id != guid && conversation.User2Id != guid) return;

        _context.Messages.RemoveRange(conversation.Messages);
        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync();

        // ✅ Notify both users
        await Clients.Group(conversationId.ToString())
            .SendAsync("ConversationDeleted", conversationId);
    }

    // ✅ Typing indicator
    public async Task UserTyping(Guid conversationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        // ✅ Only notify *other* users in the same group
        await Clients.OthersInGroup(conversationId.ToString())
            .SendAsync("UserTyping", new
            {
                conversationId,
                userId
            });
    }
}


