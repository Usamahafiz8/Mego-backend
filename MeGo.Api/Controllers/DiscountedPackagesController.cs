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
    public class DiscountedPackagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DiscountedPackagesController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
        }

        // Get all active discounted packages
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _context.DiscountedPackages
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.IsPopular)
                .ThenBy(p => p.DiscountedPrice)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    originalPrice = p.OriginalPrice,
                    discountedPrice = p.DiscountedPrice,
                    discountPercentage = p.DiscountPercentage,
                    durationDays = p.DurationDays,
                    packageType = p.PackageType,
                    pointsCost = p.PointsCost,
                    cashPrice = p.CashPrice,
                    isPopular = p.IsPopular,
                    imageUrl = p.ImageUrl,
                    features = p.Features,
                })
                .ToListAsync();

            return Ok(packages);
        }

        // Get package by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackage(int id)
        {
            var package = await _context.DiscountedPackages.FindAsync(id);
            if (package == null || !package.IsActive)
                return NotFound();

            return Ok(new
            {
                id = package.Id,
                name = package.Name,
                description = package.Description,
                originalPrice = package.OriginalPrice,
                discountedPrice = package.DiscountedPrice,
                discountPercentage = package.DiscountPercentage,
                durationDays = package.DurationDays,
                packageType = package.PackageType,
                pointsCost = package.PointsCost,
                cashPrice = package.CashPrice,
                isPopular = package.IsPopular,
                imageUrl = package.ImageUrl,
                features = package.Features,
            });
        }

        // Purchase package
        [HttpPost("{id}/purchase")]
        [Authorize]
        public async Task<IActionResult> PurchasePackage(int id, [FromBody] PurchasePackageDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var package = await _context.DiscountedPackages.FindAsync(id);
            if (package == null || !package.IsActive)
                return NotFound("Package not found");

            if (dto.Method == "points")
            {
                // Check if user has enough points
                var userPoints = await _context.UserPoints
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (userPoints == null || userPoints.AvailablePoints < package.PointsCost)
                    return BadRequest("Insufficient points");

                userPoints.AvailablePoints -= package.PointsCost;
                userPoints.LastUpdated = DateTime.UtcNow;
            }

            var purchase = new UserPackagePurchase
            {
                UserId = userId.Value,
                PackageId = package.Id,
                PurchaseMethod = dto.Method,
                AmountPaid = dto.Method == "points" ? 0 : package.CashPrice ?? 0,
                PointsUsed = dto.Method == "points" ? package.PointsCost : 0,
                PurchasedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(package.DurationDays),
                IsActive = true,
            };

            _context.UserPackagePurchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Package purchased successfully",
                purchaseId = purchase.Id,
                validUntil = purchase.ValidUntil,
            });
        }

        // Get user's purchased packages
        [HttpGet("my-packages")]
        [Authorize]
        public async Task<IActionResult> GetMyPackages()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var purchases = await _context.UserPackagePurchases
                .Include(p => p.Package)
                .Where(p => p.UserId == userId.Value && p.IsActive)
                .OrderByDescending(p => p.PurchasedAt)
                .Select(p => new
                {
                    id = p.Id,
                    packageId = p.PackageId,
                    packageName = p.Package.Name,
                    packageType = p.Package.PackageType,
                    purchaseMethod = p.PurchaseMethod,
                    purchasedAt = p.PurchasedAt,
                    validUntil = p.ValidUntil,
                    isActive = p.IsActive && p.ValidUntil > DateTime.UtcNow,
                })
                .ToListAsync();

            return Ok(purchases);
        }
    }

    public class PurchasePackageDto
    {
        public string Method { get; set; } = ""; // "points" or "cash"
    }
}

