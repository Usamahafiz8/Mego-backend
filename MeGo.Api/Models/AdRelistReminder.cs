using System;

namespace MeGo.Api.Models
{
    public class AdRelistReminder
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public DateTime LastActiveAt { get; set; }
        public DateTime? ReminderSentAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Days of inactivity before reminder
        public int InactiveDays { get; set; } = 7;
    }
}

