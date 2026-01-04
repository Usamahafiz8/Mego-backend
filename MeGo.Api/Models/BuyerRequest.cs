using System;

namespace MeGo.Api.Models
{
    public class BuyerRequest
    {
        public int Id { get; set; }
        
        public Guid BuyerId { get; set; }
        public User Buyer { get; set; } = null!;
        
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public decimal? MaxPrice { get; set; }
        
        // Status: active, fulfilled, closed
        public string Status { get; set; } = "active";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        
        // Responses from sellers
        public List<BuyerRequestResponse> Responses { get; set; } = new();
    }
    
    public class BuyerRequestResponse
    {
        public int Id { get; set; }
        public int BuyerRequestId { get; set; }
        public BuyerRequest BuyerRequest { get; set; } = null!;
        
        public Guid SellerId { get; set; }
        public User Seller { get; set; } = null!;
        
        public int? AdId { get; set; } // Optional: seller can link their ad
        public Ad? Ad { get; set; }
        
        public string Message { get; set; } = "";
        public decimal? Price { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

