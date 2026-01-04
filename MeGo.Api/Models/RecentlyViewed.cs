using System;

namespace MeGo.Api.Models
{
    public class RecentlyViewed
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}

