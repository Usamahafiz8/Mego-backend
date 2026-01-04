using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MeGo.Api.Data;
using MeGo.Api.Hubs;
using MeGo.Api.Models;
using MeGo.Api.Services;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/report")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AdminHub> _hub;
        private readonly SpamDetectionService _spamService;

        public ReportsController(AppDbContext context, IHubContext<AdminHub> hub, SpamDetectionService spamService)
        {
            _context = context;
            _hub = hub;
            _spamService = spamService;
        }

        // ✅ POST: v1/report
        [HttpPost]
        public async Task<IActionResult> ReportListing([FromBody] ReportDto dto)
        {
            try
            {
                Console.WriteLine($"📩 Incoming report: AdId={dto.AdId}, UserId={dto.UserId}, Reason={dto.Reason}");

                if (dto == null)
                    return BadRequest(new { message = "Report data is missing" });

                if (dto.AdId <= 0)
                    return BadRequest(new { message = "Invalid Ad ID" });

                if (string.IsNullOrWhiteSpace(dto.Reason))
                    return BadRequest(new { message = "Reason is required" });

                // ✅ Validate Ad
                var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == dto.AdId);
                if (ad == null)
                {
                    Console.WriteLine($"❌ Ad not found for ID {dto.AdId}");
                    return NotFound(new { message = "Ad not found" });
                }

                // ✅ Validate User
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
                if (user == null)
                {
                    Console.WriteLine($"❌ User not found for ID {dto.UserId}");
                    return NotFound(new { message = "User not found" });
                }

                // ✅ Create new report
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    AdId = dto.AdId,
                    UserId = dto.UserId,
                    Reason = dto.Reason.Trim(),
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Report saved: ReportId={report.Id}, AdId={report.AdId}, UserId={report.UserId}");

                // Check and handle spam/fraud detection
                await _spamService.CheckAndHandleSpamReports(dto.AdId);

                // ✅ Broadcast to all connected admin dashboards
                Console.WriteLine($"📡 Broadcasting report to AdminHub: {report.Id}");
                await _hub.Clients.All.SendAsync("NewReportAdded", new
                {
                    report.Id,
                    report.Reason,
                    report.Status,
                    UserName = user.Name,
                    AdTitle = ad.Title,
                    report.CreatedAt
                });

                return Ok(new { message = "Report submitted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Report error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class ReportDto
    {
        public int AdId { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; } = "";
    }
}
