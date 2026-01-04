using System;
using System.ComponentModel.DataAnnotations.Schema; // ✅ required for [ForeignKey]

namespace MeGo.Api.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ User relationship
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        // ✅ Ad relationship
        [ForeignKey("AdId")]
        public int AdId { get; set; }
        public Ad Ad { get; set; }
    }
}
