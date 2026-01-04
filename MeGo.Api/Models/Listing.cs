using System;
using System.Collections.Generic;

namespace MeGo.Api.Models
{
public class Listing
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string Status { get; set; } = "pending"; // pending, active, rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; }
}
}