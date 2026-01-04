using System.ComponentModel.DataAnnotations;

namespace MeGo.Api.Models;

public class SupportRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // Navigation property
    public User? User { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = "";

    public string? ImagePath { get; set; }

    public string Status { get; set; } = "Pending";

    public string? AdminReply { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
