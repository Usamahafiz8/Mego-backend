using System;

namespace MeGo.Api.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = ""; // Display ID like "456"
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public string Status { get; set; } = "active"; // active, scheduled, expired, completed, cancelled
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        // Shipping/Delivery info
        public string? ShippingAddress { get; set; }
        public string? DeliveryMethod { get; set; } // pickup, delivery
        public string? TrackingNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}



