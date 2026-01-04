using System;

namespace MeGo.Api.Models
{
    public class BillingInfo
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string CustomerType { get; set; } = ""; // "individual" or "business"
        public string Email { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string? BusinessName { get; set; }
        public string PhoneNumber { get; set; } = "";
        
        // Address
        public string AddressLine { get; set; } = "";
        public string City { get; set; } = "";
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}



