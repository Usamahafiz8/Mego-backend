using System;

namespace MeGo.Api.Models
{
    public class SellerRating
    {
        public int Id { get; set; }
        
        // Buyer who rated
        public Guid RaterId { get; set; }
        public User Rater { get; set; } = null!;
        
        // Seller who was rated
        public Guid SellerId { get; set; }
        public User Seller { get; set; } = null!;
        
        // Conversation/Ad context
        public Guid? ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
        
        public int? AdId { get; set; }
        public Ad? Ad { get; set; }
        
        // Rating (1-5 stars)
        public int Rating { get; set; }
        public string? Review { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

