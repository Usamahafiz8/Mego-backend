using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MeGo.Api.Data;
using MeGo.Api.Models;
using MeGo.Api.Hubs;
using MeGo.Api.Services;
using System.Security.Claims;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<AdminHub> _hub;
        private readonly AdQualityScoreService _qualityService;
        private readonly SpamDetectionService _spamService;

        public AdsController(AppDbContext context, IWebHostEnvironment env, IHubContext<AdminHub> hub, 
            AdQualityScoreService qualityService, SpamDetectionService spamService)
        {
            _context = context;
            _env = env;
            _hub = hub;
            _qualityService = qualityService;
            _spamService = spamService;
        }

        // ---------------------------------------------------------
        // ✅ CREATE AD
        // ---------------------------------------------------------
        [HttpPost]
        [Authorize]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> CreateAd([FromForm] AdCreateDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Image is required");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            var userId = Guid.Parse(userIdString);

            var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.Image.CopyToAsync(stream);

            var ad = new Ad
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Negotiable = dto.Negotiable,
                Category = dto.Category,
                Location = dto.Location,
                Contact = dto.Contact,
                Condition = dto.Condition,
                AdType = dto.AdType,
                ImageUrl = $"/uploads/{fileName}",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Status = "approved", // Auto-approve for now (can be changed to "pending" for moderation)
                IsActive = true,
                ApprovedAt = DateTime.UtcNow // Set approval timestamp
            };

            // Handle voice description if provided
            if (dto.VoiceDescription != null && dto.VoiceDescription.Length > 0)
            {
                var voiceFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.VoiceDescription.FileName)}";
                var voiceFilePath = Path.Combine(uploadsPath, voiceFileName);
                
                using (var stream = new FileStream(voiceFilePath, FileMode.Create))
                    await dto.VoiceDescription.CopyToAsync(stream);
                
                ad.VoiceDescriptionUrl = $"/uploads/{voiceFileName}";
            }

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            _context.Media.Add(new Media
            {
                FileName = fileName,
                FilePath = $"/uploads/{fileName}",
                MediaType = dto.Image.ContentType,
                AdId = ad.Id
            });

            await _context.SaveChangesAsync();

            // Record ad history
            await AdHistoryController.RecordAdHistory(_context, ad.Id, "created", null, ad);

            // Calculate quality score
            await _qualityService.CalculateAndSaveQualityScore(ad.Id);

            // Initialize analytics
            _context.AdAnalytics.Add(new AdAnalytics
            {
                AdId = ad.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Notify admin dashboards
            await _hub.Clients.All.SendAsync("NewListingAdded", new
            {
                ad.Id,
                ad.Title,
                ad.Price,
                ad.Status,
                ad.CreatedAt
            });

            return Ok(ad);
        }

        // ---------------------------------------------------------
        // ✅ MARK AD AS SOLD
        // ---------------------------------------------------------
        [HttpPost("{id}/sold")]
        [Authorize]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (ad == null) return NotFound();

            ad.Status = "sold";
            ad.IsActive = false;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Ad marked as sold" });
        }

        // ---------------------------------------------------------
        // ✅ GET ALL APPROVED/ACTIVE ADS (Public List)
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAllAds()
        {
            // Include approved, active, and pending ads (pending for development/testing)
            var ads = await _context.Ads
                .Include(a => a.Media)
                .Include(a => a.User)
                .Where(a => (a.Status.ToLower() == "approved" || a.Status.ToLower() == "active" || a.Status.ToLower() == "pending") && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(ads);
        }

        // ---------------------------------------------------------
        // ✅ GET AD BY ID
        // ---------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdById(int id)
        {
            var ad = await _context.Ads
                .Include(a => a.Media)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null)
                return NotFound();

            return Ok(ad);
        }

        // ---------------------------------------------------------
        // ✅ GET MY ADS (User Dashboard)
        // ---------------------------------------------------------
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyAds()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ads = await _context.Ads
                .Where(a => a.UserId == userId)
                .Include(a => a.Media)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    price = a.Price,
                    category = a.Category,
                    location = a.Location,
                    contact = a.Contact,
                    imageUrl = a.ImageUrl,
                    createdAt = a.CreatedAt,
                    status = a.Status,

                    // ⭐ Important for MyAds screen
                    rejectedReason = a.RejectedReason,
                    rejectedAt = a.RejectedAt,

                    media = a.Media.Select(m => new
                    {
                        filePath = m.FilePath,
                        mediaType = m.MediaType
                    })
                })
                .ToListAsync();

            return Ok(ads);
        }

        // ---------------------------------------------------------
        // ✅ UPDATE AD
        // ---------------------------------------------------------
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAd(int id, [FromForm] AdUpdateDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.Include(a => a.Media).FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return NotFound();

            if (ad.UserId != userId)
                return Forbid();

            // Save previous state for history
            var previousAd = new Ad
            {
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                Status = ad.Status
            };

            // If user edits the ad after rejection → take it back to pending
            ad.Status = "pending";
            ad.RejectedReason = null;
            ad.RejectedAt = null;

            ad.Title = dto.Title ?? ad.Title;
            ad.Description = dto.Description ?? ad.Description;
            ad.Price = dto.Price ?? ad.Price;
            ad.Negotiable = dto.Negotiable ?? ad.Negotiable;
            ad.Category = dto.Category ?? ad.Category;
            ad.Location = dto.Location ?? ad.Location;
            ad.Contact = dto.Contact ?? ad.Contact;
            ad.Condition = dto.Condition ?? ad.Condition;
            ad.AdType = dto.AdType ?? ad.AdType;

            // Handle voice description update
            if (dto.VoiceDescription != null && dto.VoiceDescription.Length > 0)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                var voiceFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.VoiceDescription.FileName)}";
                var voiceFilePath = Path.Combine(uploadsPath, voiceFileName);
                
                using (var stream = new FileStream(voiceFilePath, FileMode.Create))
                    await dto.VoiceDescription.CopyToAsync(stream);
                
                ad.VoiceDescriptionUrl = $"/uploads/{voiceFileName}";
            }

            if (dto.Image != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var path = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                    await dto.Image.CopyToAsync(stream);

                ad.ImageUrl = $"/uploads/{fileName}";

                _context.Media.Add(new Media
                {
                    FileName = fileName,
                    FilePath = ad.ImageUrl,
                    MediaType = dto.Image.ContentType,
                    AdId = ad.Id
                });
            }

            await _context.SaveChangesAsync();

            // Record ad history
            await AdHistoryController.RecordAdHistory(_context, ad.Id, "edited", previousAd, ad);

            // Recalculate quality score
            await _qualityService.CalculateAndSaveQualityScore(ad.Id);

            return Ok(ad);
        }

        // ---------------------------------------------------------
        // ✅ DELETE AD
        // ---------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var ad = await _context.Ads.Include(a => a.Media).FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null)
                return NotFound();

            if (ad.UserId != userId)
                return Forbid();

            foreach (var media in ad.Media)
            {
                var localPath = Path.Combine(_env.WebRootPath ?? "wwwroot", media.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(localPath))
                    System.IO.File.Delete(localPath);
            }

            _context.Media.RemoveRange(ad.Media);
            _context.Ads.Remove(ad);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Ad deleted successfully" });
        }
    }

    // ---------------------------------------------------------
    // DTOs
    // ---------------------------------------------------------
    public class AdCreateDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public bool Negotiable { get; set; }
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Condition { get; set; } = "";
        public string AdType { get; set; } = "";
        public IFormFile Image { get; set; }
        public IFormFile? VoiceDescription { get; set; } // Voice description audio file
    }

    public class AdUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool? Negotiable { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
        public string? Condition { get; set; }
        public string? AdType { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? VoiceDescription { get; set; } // Voice description audio file
    }
}
