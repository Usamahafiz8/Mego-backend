using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using System.Security.Claims;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /v1/favorites/me (for authenticated user)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            if (!Guid.TryParse(userIdStr, out Guid userGuid))
                return BadRequest("Invalid user ID format");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userGuid)
                .Include(f => f.Ad)
                .ThenInclude(a => a.Media)
                .Select(f => new
                {
                    f.Ad.Id,
                    f.Ad.Title,
                    f.Ad.Price,
                    f.Ad.Category,
                    f.Ad.Location,
                    f.Ad.ImageUrl,
                    Media = f.Ad.Media.Select(m => new
                    {
                        m.Id,
                        m.FileName,
                        m.FilePath,
                        m.MediaType
                    })
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // ✅ GET: /v1/favorites?userId=abc123 (for public access)
        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("userId is required");

            if (!Guid.TryParse(userId, out Guid userGuid))
                return BadRequest("Invalid userId format");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userGuid)
                .Include(f => f.Ad)
                .ThenInclude(a => a.Media)
                .Select(f => new
                {
                    f.Ad.Id,
                    f.Ad.Title,
                    f.Ad.Price,
                    f.Ad.Category,
                    f.Ad.Location,
                    f.Ad.ImageUrl,
                    Media = f.Ad.Media.Select(m => new
                    {
                        m.Id,
                        m.FileName,
                        m.FilePath,
                        m.MediaType
                    })
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // ✅ POST: /v1/favorites/toggle (uses authenticated user)
        [HttpPost("toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteToggleDto dto)
        {
            if (dto == null || dto.AdId <= 0)
                return BadRequest("AdId is required");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            if (!Guid.TryParse(userIdStr, out Guid userGuid))
                return BadRequest("Invalid user ID format");

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.AdId == dto.AdId && f.UserId == userGuid);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Removed from favorites" });
            }

            var fav = new Favorite
            {
                AdId = dto.AdId,
                UserId = userGuid,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(fav);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites" });
        }
    }

    public class FavoriteDto
    {
        public int AdId { get; set; }
        public string UserId { get; set; } = "";
    }

    public class FavoriteToggleDto
    {
        public int AdId { get; set; }
    }
}
