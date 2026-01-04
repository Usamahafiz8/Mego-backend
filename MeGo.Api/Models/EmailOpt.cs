using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class EmailOtp
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public string Email { get; set; } = "";
        public Guid? UserId { get; set; }
        [Required, MaxLength(6)] public string Code { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; } = 0;
        public bool Used { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
