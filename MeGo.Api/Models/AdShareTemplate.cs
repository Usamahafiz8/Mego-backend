using System;

namespace MeGo.Api.Models
{
    public class AdShareTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = "";
        public string TemplateType { get; set; } = ""; // "facebook", "instagram", "whatsapp", "twitter"
        public string ImageUrl { get; set; } = ""; // Template design image
        public string? HtmlTemplate { get; set; } // For custom templates
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

