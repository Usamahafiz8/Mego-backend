using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/kyc")]
    public class AdminKycController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminKycController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // ✅ GET ALL KYC (with optional status filter)
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string status = null)
        {
            var query = _context.KycInfos
                .Include(k => k.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                var statusLower = status.ToLower();
                if (statusLower == "pending")
                    query = query.Where(k => k.Status == "Pending");
                else if (statusLower == "approved")
                    query = query.Where(k => k.Status == "Approved");
                else if (statusLower == "rejected")
                    query = query.Where(k => k.Status == "Rejected");
            }

            var results = await query
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // ---------------------------------------------------------
        // ✅ GET ALL PENDING KYC (for backward compatibility)
        // ---------------------------------------------------------
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _context.KycInfos
                .Include(k => k.User)
                .Where(k => k.Status == "Pending")
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return Ok(pending);
        }

        // ---------------------------------------------------------
        // ✅ GET SINGLE KYC DETAILS
        // ---------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKyc(Guid id)
        {
            var kyc = await _context.KycInfos
                .Include(k => k.User)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kyc == null)
                return NotFound(new { message = "KYC not found" });

            return Ok(kyc);
        }

        // ---------------------------------------------------------
        // ✅ APPROVE
        // ---------------------------------------------------------
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var kyc = await _context.KycInfos.FindAsync(id);
            if (kyc == null) return NotFound();

            kyc.Status = "Approved";
            kyc.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "KYC approved successfully!" });
        }

        // ---------------------------------------------------------
        // ❌ REJECT
        // ---------------------------------------------------------
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectKycDto dto)
        {
            var kyc = await _context.KycInfos.FindAsync(id);
            if (kyc == null) return NotFound();

            kyc.Status = "Rejected";
            kyc.RejectionReason = dto?.Reason ?? "Document verification failed";
            kyc.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "KYC rejected successfully!" });
        }
    }

    public class RejectKycDto
    {
        public string Reason { get; set; } = "";
    }
}
