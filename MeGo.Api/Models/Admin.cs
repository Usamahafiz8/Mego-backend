using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class Admin
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
