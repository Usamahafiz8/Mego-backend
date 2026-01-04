using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        public User User1 { get; set; } = null!;
        public User User2 { get; set; } = null!;

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
