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
    public class BillingInfoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BillingInfoController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Get billing info for current user
        [HttpGet]
        public async Task<IActionResult> GetBillingInfo()
        {
            var userId = GetUserId();
            var billingInfo = await _context.BillingInfos
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.IsDefault)
                .ThenByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (billingInfo == null)
                return Ok((object?)null);

            return Ok(new
            {
                id = billingInfo.Id,
                customerType = billingInfo.CustomerType,
                email = billingInfo.Email,
                customerName = billingInfo.CustomerName,
                businessName = billingInfo.BusinessName,
                phoneNumber = billingInfo.PhoneNumber,
                addressLine = billingInfo.AddressLine,
                city = billingInfo.City,
                state = billingInfo.State,
                postalCode = billingInfo.PostalCode,
                country = billingInfo.Country,
                isDefault = billingInfo.IsDefault,
            });
        }

        // Create or update billing info
        [HttpPost]
        public async Task<IActionResult> SaveBillingInfo([FromBody] BillingInfoDto dto)
        {
            var userId = GetUserId();
            
            // Check if billing info exists
            var existing = await _context.BillingInfos
                .FirstOrDefaultAsync(b => b.UserId == userId && b.IsDefault);

            if (existing != null)
            {
                // Update existing
                existing.CustomerType = dto.CustomerType;
                existing.Email = dto.Email;
                existing.CustomerName = dto.CustomerName;
                existing.BusinessName = dto.BusinessName;
                existing.PhoneNumber = dto.PhoneNumber;
                existing.AddressLine = dto.AddressLine;
                existing.City = dto.City;
                existing.State = dto.State;
                existing.PostalCode = dto.PostalCode;
                existing.Country = dto.Country;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var billingInfo = new BillingInfo
                {
                    UserId = userId,
                    CustomerType = dto.CustomerType,
                    Email = dto.Email,
                    CustomerName = dto.CustomerName,
                    BusinessName = dto.BusinessName,
                    PhoneNumber = dto.PhoneNumber,
                    AddressLine = dto.AddressLine,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    Country = dto.Country,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _context.BillingInfos.Add(billingInfo);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Billing information saved successfully" });
        }
    }

    public class BillingInfoDto
    {
        public string CustomerType { get; set; } = "";
        public string Email { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string? BusinessName { get; set; }
        public string PhoneNumber { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string City { get; set; } = "";
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}



