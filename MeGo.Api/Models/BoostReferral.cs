using System;

namespace MeGo.Api.Models
{
    public class BoostReferral
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string ShareLink { get; set; } = "";
        public int ClickCount { get; set; } = 0;
        public int RequiredClicks { get; set; } = 3; // Default: 3 clicks = free boost
        public bool BoostEarned { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? BoostEarnedAt { get; set; }
    }
}

