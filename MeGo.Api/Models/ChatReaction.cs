using System;

namespace MeGo.Api.Models
{
    public class ChatReaction
    {
        public int Id { get; set; }
        
        public Guid MessageId { get; set; }
        public Message Message { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Emoji reaction (👍, ❤️, 😂, 😮, 😢, 🙏)
        public string Emoji { get; set; } = "";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

