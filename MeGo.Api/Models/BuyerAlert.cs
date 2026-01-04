using System;

namespace MeGo.Api.Models
{
    public class BuyerAlert
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Alert criteria
        public string? Category { get; set; }
        public string? Location { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Keywords { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastTriggeredAt { get; set; }
    }
}

