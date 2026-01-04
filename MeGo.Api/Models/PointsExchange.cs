using System;

namespace MeGo.Api.Models
{
    public class PointsExchange
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string ExchangeType { get; set; } = ""; // "coins", "boost", "mobile_recharge", "withdrawal"
        public int PointsUsed { get; set; }
        public int ValueReceived { get; set; } // Coins, boosts, etc.
        
        // For mobile recharge
        public string? MobileNetwork { get; set; } // "Jazz", "Zong", "Telenor"
        public string? MobileNumber { get; set; }
        public string? TransactionId { get; set; }
        
        public string Status { get; set; } = "pending"; // pending, completed, failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}

