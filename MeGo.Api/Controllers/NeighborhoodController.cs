using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using System;
using System.Linq;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class NeighborhoodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NeighborhoodController(AppDbContext context)
        {
            _context = context;
        }

        // Get neighborhood feed (location-based listings and buyer requests)
        [HttpGet("feed")]
        public async Task<IActionResult> GetNeighborhoodFeed([FromQuery] string location, [FromQuery] int radiusKm = 10)
        {
            if (string.IsNullOrEmpty(location))
                return BadRequest("Location is required");

            var normalizedLocation = location.Trim().ToLower();

            // Get ads in the same location
            var ads = await _context.Ads
                .Where(a => a.Status == "approved" && a.IsActive &&
                    !string.IsNullOrEmpty(a.Location) &&
                    a.Location.ToLower().Contains(normalizedLocation))
                .Include(a => a.Media)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Price,
                    a.Location,
                    a.ImageUrl,
                    a.CreatedAt,
                    a.IsBoosted,
                    media = a.Media.Select(m => new { m.FilePath, m.MediaType }),
                    seller = new { a.User.Id, a.User.Name, a.User.ProfileImage }
                })
                .ToListAsync();

            // Get active buyer requests in the same location
            var buyerRequests = await _context.BuyerRequests
                .Where(b => b.Status == "active" &&
                    !string.IsNullOrEmpty(b.Location) &&
                    b.Location.ToLower().Contains(normalizedLocation))
                .Include(b => b.Buyer)
                .OrderByDescending(b => b.CreatedAt)
                .Take(20)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.Category,
                    b.Location,
                    b.MaxPrice,
                    b.CreatedAt,
                    buyer = new { b.Buyer.Id, b.Buyer.Name }
                })
                .ToListAsync();

            return Ok(new
            {
                location,
                ads,
                buyerRequests,
                timestamp = DateTime.UtcNow
            });
        }

        // Get nearby listings (for map view)
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyListings([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radiusKm = 10)
        {
            // Note: This is a simplified version. For production, use proper geolocation calculations
            var ads = await _context.Ads
                .Where(a => a.Status == "approved" && a.IsActive)
                .Include(a => a.Media)
                .Include(a => a.User)
                .Take(100)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Price,
                    a.Location,
                    a.ImageUrl,
                    a.CreatedAt,
                    media = a.Media.Select(m => new { m.FilePath })
                })
                .ToListAsync();

            return Ok(ads);
        }
    }
}

