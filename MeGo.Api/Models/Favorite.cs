using System;
using System.Collections.Generic;
namespace MeGo.Api.Models{
public class Favorite
{
    public int Id { get; set; }
    public int AdId { get; set; }
    public Guid UserId { get; set; }   // 👈 Guid not string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Ad Ad { get; set; }
    public User User { get; set; }
}
}