using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class KycInfo
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }  // 🔥 Link to User

        public string CnicNumber { get; set; } = "";
        public string CnicFrontImageUrl { get; set; } = "";
        public string CnicBackImageUrl { get; set; } = "";
        public string SelfieUrl { get; set; } = "";

        // Live Verification (Advanced tier)
        public string? LiveVerificationVideoUrl { get; set; } // Video recording for live verification
        public DateTime? LiveVerificationScheduledAt { get; set; } // Scheduled live verification time
        public string? LiveVerificationSessionId { get; set; } // Session ID for live verification
        public string VerificationTier { get; set; } = "Basic"; // Basic, Intermediate, Advanced

        public string Status { get; set; } = "Pending"; 
        public string? RejectionReason { get; set; } // 🔥 For admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public User? User { get; set; } // Navigation
    }
}
