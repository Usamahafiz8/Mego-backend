using System;
using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models
{
    public class TaskHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string TaskType { get; set; } = ""; // e.g. "DailyLogin", "PostAd", "ReferFriend"
        public int PointsEarned { get; set; } = 0;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
