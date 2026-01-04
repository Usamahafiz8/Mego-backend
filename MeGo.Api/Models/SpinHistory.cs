using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class SpinHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string PrizeType { get; set; } = ""; // e.g. "Points", "Coin", "Boost"
        public int PrizeValue { get; set; } = 0;
        public DateTime SpinDate { get; set; } = DateTime.UtcNow;
    }
}
