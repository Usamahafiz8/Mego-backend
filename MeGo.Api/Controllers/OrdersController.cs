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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Get all orders for current user
        [HttpGet]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status)
        {
            var userId = GetUserId();
            var query = _context.Orders
                .Include(o => o.Ad)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status.ToLower() == status.ToLower());
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    id = o.Id,
                    orderId = o.OrderId,
                    productName = o.ProductName,
                    price = o.Price,
                    status = o.Status,
                    date = o.OrderDate.ToString("dd MMM yyyy"),
                    scheduledDate = o.ScheduledDate,
                    expiryDate = o.ExpiryDate,
                    image = o.Ad != null ? o.Ad.ImageUrl : null,
                })
                .ToListAsync();

            return Ok(orders);
        }

        // Get order by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = GetUserId();
            var order = await _context.Orders
                .Include(o => o.Ad)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return Ok(new
            {
                id = order.Id,
                orderId = order.OrderId,
                productName = order.ProductName,
                price = order.Price,
                status = order.Status,
                date = order.OrderDate,
                scheduledDate = order.ScheduledDate,
                expiryDate = order.ExpiryDate,
                completedDate = order.CompletedDate,
                shippingAddress = order.ShippingAddress,
                deliveryMethod = order.DeliveryMethod,
                trackingNumber = order.TrackingNumber,
                image = order.Ad != null ? order.Ad.ImageUrl : null,
            });
        }

        // Create order (typically called when ad is purchased)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = GetUserId();
            var ad = await _context.Ads.FindAsync(dto.AdId);
            if (ad == null)
                return NotFound("Ad not found");

            var orderId = new Random().Next(100, 999).ToString();
            
            var order = new Order
            {
                OrderId = orderId,
                UserId = userId,
                AdId = dto.AdId,
                ProductName = dto.ProductName ?? ad.Title,
                Price = dto.Price ?? ad.Price,
                Status = dto.Status ?? "active",
                OrderDate = DateTime.UtcNow,
                ScheduledDate = dto.ScheduledDate,
                ExpiryDate = dto.ExpiryDate,
                ShippingAddress = dto.ShippingAddress,
                DeliveryMethod = dto.DeliveryMethod,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = order.Id,
                orderId = order.OrderId,
                productName = order.ProductName,
                price = order.Price,
                status = order.Status,
            });
        }
    }

    public class CreateOrderDto
    {
        public int AdId { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ShippingAddress { get; set; }
        public string? DeliveryMethod { get; set; }
    }
}



