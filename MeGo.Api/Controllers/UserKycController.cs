using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using MeGo.Api.DTOs;
using System.Security.Claims;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/kyc")]
    [Authorize]
    public class UserKycController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserKycController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Get KYC status
        [HttpGet("status")]
        public async Task<IActionResult> GetMyKycStatus()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var kyc = await _context.KycInfos
                .FirstOrDefaultAsync(k => k.UserId == userId);

            if (kyc == null)
                return Ok(new { status = "NotSubmitted", verificationTier = "Basic" });

            return Ok(new
            {
                status = kyc.Status,
                verificationTier = kyc.VerificationTier,
                rejectionReason = kyc.RejectionReason,
                cnic = kyc.CnicNumber,
                front = kyc.CnicFrontImageUrl,
                back = kyc.CnicBackImageUrl,
                selfie = kyc.SelfieUrl,
                liveVerificationVideoUrl = kyc.LiveVerificationVideoUrl,
                liveVerificationScheduledAt = kyc.LiveVerificationScheduledAt
            });
        }

        // Submit Basic/Intermediate KYC (CNIC + Selfie)
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitKyc([FromForm] DTOs.KycSubmissionDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            string frontUrl = await SaveImage(dto.CnicFrontImage);
            string backUrl = await SaveImage(dto.CnicBackImage);
            string selfieUrl = await SaveImage(dto.Selfie);

            var existing = await _context.KycInfos.FirstOrDefaultAsync(k => k.UserId == userId);

            if (existing != null)
            {
                existing.CnicNumber = dto.CnicNumber;
                existing.CnicFrontImageUrl = frontUrl;
                existing.CnicBackImageUrl = backUrl;
                existing.SelfieUrl = selfieUrl;
                existing.VerificationTier = dto.VerificationTier;
                existing.Status = "Pending";
                existing.RejectionReason = null;
            }
            else
            {
                _context.KycInfos.Add(new KycInfo
                {
                    UserId = userId,
                    CnicNumber = dto.CnicNumber,
                    CnicFrontImageUrl = frontUrl,
                    CnicBackImageUrl = backUrl,
                    SelfieUrl = selfieUrl,
                    VerificationTier = dto.VerificationTier,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "KYC submitted successfully!" });
        }

        // Submit Live Verification (Advanced tier)
        [HttpPost("submit-live-verification")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitLiveVerification([FromForm] IFormFile? LiveVideo)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var kyc = await _context.KycInfos.FirstOrDefaultAsync(k => k.UserId == userId);
            if (kyc == null)
                return BadRequest("Please submit basic KYC first");

            if (kyc.Status != "Approved")
                return BadRequest("Basic KYC must be approved before live verification");

            string videoUrl = await SaveVideo(LiveVideo);

            kyc.LiveVerificationVideoUrl = videoUrl;
            kyc.VerificationTier = "Advanced";
            kyc.Status = "Pending"; // Re-review for advanced tier
            kyc.LiveVerificationScheduledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update user verification tier
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.VerificationTier = 2; // Advanced
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Live verification submitted successfully!" });
        }

        // Schedule live verification session
        [HttpPost("schedule-live-verification")]
        public async Task<IActionResult> ScheduleLiveVerification([FromBody] ScheduleLiveVerificationDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var kyc = await _context.KycInfos.FirstOrDefaultAsync(k => k.UserId == userId);
            if (kyc == null)
                return BadRequest("Please submit basic KYC first");

            kyc.LiveVerificationScheduledAt = dto.ScheduledAt;
            kyc.LiveVerificationSessionId = Guid.NewGuid().ToString();

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Live verification scheduled",
                sessionId = kyc.LiveVerificationSessionId,
                scheduledAt = kyc.LiveVerificationScheduledAt
            });
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return "";

            string folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "kyc");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/kyc/" + fileName;
        }

        private async Task<string> SaveVideo(IFormFile file)
        {
            if (file == null || file.Length == 0) return "";

            string folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "kyc", "live-verification");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/kyc/live-verification/" + fileName;
        }
    }

    public class ScheduleLiveVerificationDto
    {
        public DateTime ScheduledAt { get; set; }
    }
}

