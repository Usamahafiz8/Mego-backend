using System;

namespace MeGo.Api.Models
{
    public class AdQualityScore
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        // Score components (0-100 each)
        public int TitleScore { get; set; } = 0;      // Based on length, keywords
        public int ImageScore { get; set; } = 0;      // Based on number and quality
        public int DescriptionScore { get; set; } = 0; // Based on completeness
        public int CompletenessScore { get; set; } = 0; // Based on all fields filled
        
        // Overall score (0-100)
        public int OverallScore { get; set; } = 0;
        
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
    }
}

