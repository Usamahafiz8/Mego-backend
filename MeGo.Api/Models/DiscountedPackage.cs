using System;

namespace MeGo.Api.Models
{
    public class DiscountedPackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        
        public int DurationDays { get; set; } // Package validity period
        public string PackageType { get; set; } = ""; // "premium", "featured", "boost", etc.
        
        public int PointsCost { get; set; } // Can also be purchased with points
        public decimal? CashPrice { get; set; } // Optional cash price
        
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; } = false;
        
        public string? ImageUrl { get; set; }
        public string? Features { get; set; } // JSON array of features
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } // Package expiry date
    }
    
    public class UserPackagePurchase
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int PackageId { get; set; }
        public DiscountedPackage Package { get; set; } = null!;
        
        public string PurchaseMethod { get; set; } = ""; // "points", "cash"
        public decimal AmountPaid { get; set; }
        public int PointsUsed { get; set; }
        
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; } = true;
    }
}



