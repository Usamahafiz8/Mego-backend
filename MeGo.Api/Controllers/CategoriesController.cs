using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;

namespace MeGo.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    adCount = _context.Ads.Count(a => a.Category == c.Name && a.IsActive && (a.Status == "approved" || a.Status == "active"))
                })
                .OrderBy(c => c.name)
                .ToListAsync();
            
            return Ok(categories);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
            
            if (category == null)
                return NotFound();
            
            var adCount = await _context.Ads
                .CountAsync(a => a.Category == category.Name && a.IsActive && (a.Status == "approved" || a.Status == "active"));
            
            return Ok(new
            {
                id = category.Id,
                name = category.Name,
                slug = category.Slug,
                adCount = adCount
            });
        }
    }
}
