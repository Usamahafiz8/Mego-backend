using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeGo.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AdminAuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ Register a new admin (one-time setup)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterDto dto)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                return BadRequest(new { message = "Email already registered" });

            var admin = new MeGo.Api.Models.Admin
            {
                Name = dto.Name,
                Email = dto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin registered successfully" });
        }

        // ✅ Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email.ToLower());
            if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            var token = GenerateJwtToken(admin);
            return Ok(new
            {
                token,
                admin.Id,
                admin.Name,
                admin.Email
            });
        }

        // ✅ Generate JWT
        private string GenerateJwtToken(MeGo.Api.Models.Admin admin)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ✅ DTOs
    public class AdminRegisterDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AdminLoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
