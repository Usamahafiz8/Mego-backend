using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class Referral
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReferrerId { get; set; }
        public Guid? ReferredUserId { get; set; }
        [Required] public string ReferralCode { get; set; } = "";
        public bool RewardGiven { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
