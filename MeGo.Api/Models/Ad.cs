using System;
using System.Collections.Generic;

namespace MeGo.Api.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public bool Negotiable { get; set; }
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Condition { get; set; } = "";
        public string AdType { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔥 Moderation status: pending / approved / rejected
        public string Status { get; set; } = "pending";

        // 🔥 Ad visibility on platform
        public bool IsActive { get; set; } = true;

        // 🔥 Admin moderation timestamps
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        // 🔥 Reason shown only when rejected
        public string? RejectedReason { get; set; }

        // 🔥 Owner
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public List<Favorite> Favorites { get; set; }
        // public List<Media> Media { get; set; } = new();
    //   public List<Favorite> Favorites { get; set; } = new();
        public List<Report> Reports { get; set; } = new();
        public List<Media> Media { get; set; } = new List<Media>();

        // New features
        public string? VoiceDescriptionUrl { get; set; } // Voice description audio file
        public bool IsBoosted { get; set; } = false;
        public DateTime? BoostedUntil { get; set; }
        public int ViewCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;
        public int SaveCount { get; set; } = 0;
        public int ShareCount { get; set; } = 0;
        public DateTime? LastRepostedAt { get; set; }
        public bool IsSpam { get; set; } = false;
        public bool IsFraud { get; set; } = false;
        public int SpamReportCount { get; set; } = 0;
        public int FraudReportCount { get; set; } = 0;
        public DateTime? AutoHiddenAt { get; set; }
    }
}
