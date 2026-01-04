using System;

namespace MeGo.Api.Models
{
    public class AdHistory
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public string Action { get; set; } = ""; // "created", "edited", "boosted", "reposted", "deleted"
        public string? PreviousValue { get; set; } // JSON snapshot of previous state
        public string? NewValue { get; set; } // JSON snapshot of new state
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

