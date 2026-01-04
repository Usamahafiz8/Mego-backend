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
    public class BuyerRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuyerRequestController(AppDbContext context)
        {
            _context = context;
        }

        // Create buyer request ("What to Buy")
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBuyerRequest([FromBody] CreateBuyerRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var request = new BuyerRequest
            {
                BuyerId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Location = dto.Location,
                MaxPrice = dto.MaxPrice,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30) // Default 30 days
            };

            _context.BuyerRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(request);
        }

        // Get all active buyer requests
        [HttpGet]
        public async Task<IActionResult> GetBuyerRequests([FromQuery] string? category, [FromQuery] string? location)
        {
            var query = _context.BuyerRequests
                .Where(b => b.Status == "active")
                .Include(b => b.Buyer)
                .Include(b => b.Responses)
                    .ThenInclude(r => r.Seller)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(b => b.Category == category);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(b => b.Location.Contains(location));

            var requests = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // Get my buyer requests
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBuyerRequests()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var requests = await _context.BuyerRequests
                .Where(b => b.BuyerId == userId)
                .Include(b => b.Responses)
                    .ThenInclude(r => r.Seller)
                .Include(b => b.Responses)
                    .ThenInclude(r => r.Ad)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // Respond to buyer request (seller)
        [HttpPost("{requestId}/respond")]
        [Authorize]
        public async Task<IActionResult> RespondToBuyerRequest(int requestId, [FromBody] RespondToBuyerRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var buyerRequest = await _context.BuyerRequests.FindAsync(requestId);
            if (buyerRequest == null) return NotFound();

            if (buyerRequest.BuyerId == userId)
                return BadRequest("Cannot respond to your own request");

            var response = new BuyerRequestResponse
            {
                BuyerRequestId = requestId,
                SellerId = userId,
                AdId = dto.AdId,
                Message = dto.Message,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow
            };

            _context.BuyerRequestResponses.Add(response);
            await _context.SaveChangesAsync();

            return Ok(response);
        }

        // Close buyer request
        [HttpPost("{requestId}/close")]
        [Authorize]
        public async Task<IActionResult> CloseBuyerRequest(int requestId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var buyerRequest = await _context.BuyerRequests.FindAsync(requestId);
            if (buyerRequest == null) return NotFound();

            if (buyerRequest.BuyerId != userId) return Forbid();

            buyerRequest.Status = "closed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Buyer request closed" });
        }
    }

    public class CreateBuyerRequestDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public decimal? MaxPrice { get; set; }
    }

    public class RespondToBuyerRequestDto
    {
        public int? AdId { get; set; }
        public string Message { get; set; } = "";
        public decimal? Price { get; set; }
    }
}

