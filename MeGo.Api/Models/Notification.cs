using System;

namespace MeGo.Api.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }        // null => global notification
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "info"; // e.g. info, warning, alert
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
