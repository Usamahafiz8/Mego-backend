// Models/Media.cs
using System;

namespace MeGo.Api.Models
{
    public class Media
    {
        public int Id { get; set; }
        public string FileName { get; set; }   // e.g. "photo1.jpg"
        public string FilePath { get; set; }   // e.g. "uploads/photo1.jpg"
        public string MediaType { get; set; }  // e.g. "image/png" or "video/mp4"
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Relationship (optional)
        public int? AdId { get; set; }
        public Ad Ad { get; set; }
    }
}
