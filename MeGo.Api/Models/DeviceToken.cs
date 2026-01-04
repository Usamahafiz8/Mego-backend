using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class DeviceToken
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Platform { get; set; } 
    }
}
