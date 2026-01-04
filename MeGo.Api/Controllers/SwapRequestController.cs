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
    [Authorize]
    public class SwapRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SwapRequestController(AppDbContext context)
        {
            _context = context;
        }

        // Create swap request
        [HttpPost]
        public async Task<IActionResult> CreateSwapRequest([FromBody] CreateSwapRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            // Verify both ads exist and belong to different users
            var requesterAd = await _context.Ads.FindAsync(dto.RequesterAdId);
            var targetAd = await _context.Ads.FindAsync(dto.TargetAdId);

            if (requesterAd == null || targetAd == null)
                return NotFound("One or both ads not found");

            if (requesterAd.UserId != userId)
                return Forbid("You don't own the requester ad");

            if (requesterAd.UserId == targetAd.UserId)
                return BadRequest("Cannot swap with your own ad");

            var swapRequest = new SwapRequest
            {
                RequesterId = userId,
                RequesterAdId = dto.RequesterAdId,
                TargetAdId = dto.TargetAdId,
                Message = dto.Message,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.SwapRequests.Add(swapRequest);
            await _context.SaveChangesAsync();

            return Ok(swapRequest);
        }

        // Get swap requests for my ads
        [HttpGet("my-ads")]
        public async Task<IActionResult> GetSwapRequestsForMyAds()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var myAdIds = await _context.Ads
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            var requests = await _context.SwapRequests
                .Where(s => myAdIds.Contains(s.TargetAdId) || myAdIds.Contains(s.RequesterAdId))
                .Include(s => s.RequesterAd)
                    .ThenInclude(a => a.Media)
                .Include(s => s.TargetAd)
                    .ThenInclude(a => a.Media)
                .Include(s => s.Requester)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        // Accept swap request
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptSwapRequest(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var swapRequest = await _context.SwapRequests
                .Include(s => s.TargetAd)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (swapRequest == null) return NotFound();

            if (swapRequest.TargetAd.UserId != userId)
                return Forbid("You don't own the target ad");

            if (swapRequest.Status != "pending")
                return BadRequest("Swap request already processed");

            swapRequest.Status = "accepted";
            swapRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Swap request accepted" });
        }

        // Reject swap request
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectSwapRequest(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var swapRequest = await _context.SwapRequests
                .Include(s => s.TargetAd)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (swapRequest == null) return NotFound();

            if (swapRequest.TargetAd.UserId != userId)
                return Forbid("You don't own the target ad");

            if (swapRequest.Status != "pending")
                return BadRequest("Swap request already processed");

            swapRequest.Status = "rejected";
            swapRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Swap request rejected" });
        }
    }

    public class CreateSwapRequestDto
    {
        public int RequesterAdId { get; set; }
        public int TargetAdId { get; set; }
        public string? Message { get; set; }
    }
}

