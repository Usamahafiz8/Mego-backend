using System;

namespace MeGo.Api.Models
{
    public class DeliveryOrder
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = "";
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int? OrderId_FK { get; set; } // Reference to Order
        public Order? Order { get; set; }
        
        public string Status { get; set; } = "pending"; // pending, picked_up, in_transit, delivered, cancelled
        public string? TrackingNumber { get; set; }
        
        // Delivery addresses
        public string PickupAddress { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        
        // Delivery details
        public DateTime? PickupDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryPersonContact { get; set; }
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

