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
    public class DeliveryOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeliveryOrdersController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Get all delivery orders for current user
        [HttpGet]
        public async Task<IActionResult> GetDeliveryOrders([FromQuery] string? status)
        {
            var userId = GetUserId();
            var query = _context.DeliveryOrders
                .Include(d => d.Order)
                .ThenInclude(o => o.Ad)
                .Where(d => d.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.Status.ToLower() == status.ToLower());
            }

            var deliveryOrders = await query
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    id = d.Id,
                    orderId = d.OrderId,
                    status = d.Status,
                    trackingNumber = d.TrackingNumber,
                    pickupAddress = d.PickupAddress,
                    deliveryAddress = d.DeliveryAddress,
                    pickupDate = d.PickupDate,
                    estimatedDeliveryDate = d.EstimatedDeliveryDate,
                    deliveredDate = d.DeliveredDate,
                    deliveryPersonName = d.DeliveryPersonName,
                    deliveryPersonContact = d.DeliveryPersonContact,
                    notes = d.Notes,
                    productName = d.Order != null && d.Order.Ad != null ? d.Order.Ad.Title : "N/A",
                    image = d.Order != null && d.Order.Ad != null ? d.Order.Ad.ImageUrl : null,
                })
                .ToListAsync();

            return Ok(deliveryOrders);
        }

        // Get delivery order by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeliveryOrder(int id)
        {
            var userId = GetUserId();
            var deliveryOrder = await _context.DeliveryOrders
                .Include(d => d.Order)
                .ThenInclude(o => o.Ad)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (deliveryOrder == null)
                return NotFound();

            return Ok(new
            {
                id = deliveryOrder.Id,
                orderId = deliveryOrder.OrderId,
                status = deliveryOrder.Status,
                trackingNumber = deliveryOrder.TrackingNumber,
                pickupAddress = deliveryOrder.PickupAddress,
                deliveryAddress = deliveryOrder.DeliveryAddress,
                pickupDate = deliveryOrder.PickupDate,
                estimatedDeliveryDate = deliveryOrder.EstimatedDeliveryDate,
                deliveredDate = deliveryOrder.DeliveredDate,
                deliveryPersonName = deliveryOrder.DeliveryPersonName,
                deliveryPersonContact = deliveryOrder.DeliveryPersonContact,
                notes = deliveryOrder.Notes,
            });
        }
    }
}



