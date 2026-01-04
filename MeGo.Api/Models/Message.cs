using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public string? MessageType { get; set; }    
        public string? FileUrl { get; set; }
        public string Content { get; set; } = "";   
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
    }
}
