using MeGo.Api.Data;
using MeGo.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/support")]
    public class AdminSupportController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<AdminHub> _hub;

        public AdminSupportController(AppDbContext db, IHubContext<AdminHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ✅ Get all support tickets
        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets([FromQuery] string? status)
        {
            var query = _db.SupportRequests
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status.ToLower() == status.ToLower());
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    id = t.Id,
                    userId = t.UserId,
                    userName = t.User != null ? t.User.Name : "Unknown",
                    userEmail = t.User != null ? t.User.Email : "Unknown",
                    message = t.Message,
                    status = t.Status,
                    imagePath = t.ImagePath,
                    createdAt = t.CreatedAt,
                updatedAt = t.CreatedAt,
                adminResponse = t.AdminReply
                })
                .ToListAsync();

            return Ok(new { tickets });
        }

        // ✅ Get single ticket
        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicket(Guid id)
        {
            var ticket = await _db.SupportRequests
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(new { message = "Ticket not found" });

            var result = new
            {
                id = ticket.Id,
                userId = ticket.UserId,
                userName = ticket.User != null ? ticket.User.Name : "Unknown",
                userEmail = ticket.User != null ? ticket.User.Email : "Unknown",
                message = ticket.Message,
                status = ticket.Status,
                imagePath = ticket.ImagePath,
                createdAt = ticket.CreatedAt,
                updatedAt = ticket.CreatedAt,
                adminResponse = ticket.AdminReply
            };

            return Ok(result);
        }

        // ✅ Update ticket status and add admin response
        [HttpPut("tickets/{id}")]
        public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketDto dto)
        {
            var ticket = await _db.SupportRequests.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found" });

            if (!string.IsNullOrEmpty(dto.Status))
            {
                ticket.Status = dto.Status;
            }

            if (!string.IsNullOrEmpty(dto.AdminResponse))
            {
                ticket.AdminReply = dto.AdminResponse;
            }
            await _db.SaveChangesAsync();

            // 🔔 Notify admins via SignalR
            await _hub.Clients.All.SendAsync("SupportTicketUpdated", new
            {
                id = ticket.Id,
                status = ticket.Status,
                updatedAt = ticket.CreatedAt
            });

            return Ok(new { success = true, message = "Ticket updated successfully." });
        }

        // ✅ Delete ticket
        [HttpDelete("tickets/{id}")]
        public async Task<IActionResult> DeleteTicket(Guid id)
        {
            var ticket = await _db.SupportRequests.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found" });

            _db.SupportRequests.Remove(ticket);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("SupportTicketDeleted", new { id });

            return Ok(new { success = true, message = "Ticket deleted successfully." });
        }

        // ✅ Get ticket statistics
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var total = await _db.SupportRequests.CountAsync();
            var pending = await _db.SupportRequests.CountAsync(t => t.Status == "Pending");
            var resolved = await _db.SupportRequests.CountAsync(t => t.Status == "Resolved");
            var inProgress = await _db.SupportRequests.CountAsync(t => t.Status == "InProgress");
            var closed = await _db.SupportRequests.CountAsync(t => t.Status == "Closed");

            return Ok(new
            {
                total,
                pending,
                resolved,
                inProgress,
                closed
            });
        }
    }

    // ✅ DTO
    public class UpdateTicketDto
    {
        public string? Status { get; set; }
        public string? AdminResponse { get; set; }
    }
}

