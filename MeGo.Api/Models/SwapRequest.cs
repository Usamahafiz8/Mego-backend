using System;

namespace MeGo.Api.Models
{
    public class SwapRequest
    {
        public int Id { get; set; }
        
        // User who created the swap request
        public Guid RequesterId { get; set; }
        public User Requester { get; set; } = null!;
        
        // Ad that user wants to swap
        public int RequesterAdId { get; set; }
        public Ad RequesterAd { get; set; } = null!;
        
        // Ad that user wants in exchange
        public int TargetAdId { get; set; }
        public Ad TargetAd { get; set; } = null!;
        
        // Status: pending, accepted, rejected, completed
        public string Status { get; set; } = "pending";
        
        public string? Message { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}

