using System;
using System.Collections.Generic;

namespace MeGo.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public int CoinsBalance { get; set; } = 0;

        // ✅ Required by controllers
        public int VerificationTier { get; set; } = 0;   // 0 = unverified, 1 = basic, 2 = premium
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation property
        public List<Ad> Ads { get; set; } = new List<Ad>();
        // inside User class
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
        public string? ProfileImage { get; set; }   // optional URL or file path
        public bool DarkMode { get; set; } = false; // user preference
        public bool NotificationsEnabled { get; set; } = true; // user preference
        public bool HideProfile { get; set; } = false;
        public bool AllowMessages { get; set; } = true;
        public string Language { get; set; } = "en";
        public bool EmailConfirmed { get; set; } = false;
        public string Status { get; set; } = "active"; 
        public bool IsActive { get; set; } = true;
        public bool IsBanned { get; set; } = false;
        public KycInfo? KycInfo { get; set; }
        public List<Favorite> Favorites { get; set; } = new();   // ⭐ REQUIRED
        public List<Report> Reports { get; set; } = new();    

  
    }

}
