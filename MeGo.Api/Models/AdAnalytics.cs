using System;

namespace MeGo.Api.Models
{
    public class AdAnalytics
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        // Metrics
        public int Views { get; set; } = 0;
        public int Clicks { get; set; } = 0;
        public int Saves { get; set; } = 0;
        public int Shares { get; set; } = 0;
        public int Messages { get; set; } = 0;
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastViewedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}

