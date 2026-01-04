using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeviceTokensController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SaveToken([FromBody] string token)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var guid = Guid.Parse(userId);

        var existing = _context.DeviceTokens.FirstOrDefault(t => t.UserId == guid && t.Token == token);

        if (existing == null)
        {
            _context.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = guid,
                Token = token
            });
        }
        else
        {
            existing.IsActive = true;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Token saved" });
    }
}
