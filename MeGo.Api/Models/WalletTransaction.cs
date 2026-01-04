using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class WalletTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        [Required] public string Method { get; set; } = "";   // JazzCash / Easypaisa / etc
        public decimal Amount { get; set; }                   // PKR amount
        public int PointsUsed { get; set; }                   // points converted
        public string Status { get; set; } = "Pending";       // Pending / Approved / Rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
