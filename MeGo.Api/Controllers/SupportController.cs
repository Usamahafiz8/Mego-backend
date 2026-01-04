using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SupportController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ✅ Create a support ticket
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] SupportRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var guid = Guid.Parse(userId);

        var ticket = new SupportRequest
        {
            Id = Guid.NewGuid(),
            UserId = guid,
            Message = dto.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Optional image
        if (dto.Image != null && dto.Image.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads/support");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.Image.CopyToAsync(stream);

            ticket.ImagePath = $"/uploads/support/{fileName}";
        }

        _context.SupportRequests.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Support request created successfully!" });
    }

    // ✅ Fetch user’s own tickets
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var guid = Guid.Parse(userId);

        var tickets = await _context.SupportRequests
            .Where(t => t.UserId == guid)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tickets);
    }
}

// ✅ DTO
public class SupportRequestDto
{
    public string Message { get; set; } = "";
    public IFormFile? Image { get; set; }
}
