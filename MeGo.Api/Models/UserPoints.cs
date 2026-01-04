using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class UserPoints
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int TotalPoints { get; set; } = 0;
        public int AvailablePoints { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
