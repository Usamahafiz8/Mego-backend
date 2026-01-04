using System;

namespace MeGo.Api.Models
{
    public class SellerTip
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public string TipType { get; set; } = ""; // "keyword_spam", "duplicate_image", "incomplete", "low_quality"
        public string Message { get; set; } = "";
        public string Severity { get; set; } = "info"; // info, warning, error
        
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

