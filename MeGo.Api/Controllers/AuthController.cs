using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models;
using MeGo.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly EmailService _emailService;

    public AuthController(AppDbContext context, IConfiguration config, IWebHostEnvironment env, EmailService emailService)
    {
        _context = context;
        _config = config;
        _env = env;
        _emailService = emailService;
    }

    // ✅ Signup (with OTP send)
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] UserSignupDto dto)
    {
        // Check if phone or email already exists
        if (!string.IsNullOrEmpty(dto.Phone) && await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
            return BadRequest("Phone already registered");
        
        if (!string.IsNullOrEmpty(dto.Email) && await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Phone = dto.Phone ?? "",
            Email = dto.Email ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            ProfileImage = null,
            DarkMode = false,
            NotificationsEnabled = true,
            EmailConfirmed = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // ✅ Generate OTP for Email
        if (!string.IsNullOrEmpty(user.Email))
        {
            var code = new Random().Next(1000, 9999).ToString();
            var otp = new EmailOtp
            {
                Email = user.Email.Trim().ToLower(),
                UserId = user.Id,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpEmailAsync(user.Email, code);
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            id = user.Id,
            name = user.Name,
            phone = user.Phone,
            email = user.Email,
            profileImage = user.ProfileImage
        });
    }

    // ✅ Login (supports both email and phone)
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        User? user = null;
        
        // Try to find user by email or phone
        if (dto.PhoneOrEmail.Contains("@"))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.PhoneOrEmail);
        }
        else
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == dto.PhoneOrEmail);
        }
        
        if (user == null)
            return Unauthorized("Invalid credentials");
        
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Incorrect password! Please check your password.");
        
        // ✅ Block banned users
        if (user.IsBanned)
            return Unauthorized("Your account has been banned by admin");

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            id = user.Id,
            name = user.Name,
            phone = user.Phone,
            email = user.Email,
            profileImage = user.ProfileImage,
            user = new
            {
                id = user.Id,
                name = user.Name,
                phone = user.Phone,
                email = user.Email,
            }
        });
    }

    // ✅ Google OAuth Login/Signup
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthDto dto)
    {
        try
        {
            // Verify Google token and get user info
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={dto.AccessToken}");
            
            if (!response.IsSuccessStatusCode)
                return Unauthorized("Invalid Google token");

            var content = await response.Content.ReadAsStringAsync();
            var googleUser = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(content);

            if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
                return BadRequest("Failed to get Google user info");

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == googleUser.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = googleUser.Name ?? "Google User",
                    Email = googleUser.Email,
                    Phone = "", // Google doesn't provide phone
                    PasswordHash = "", // No password for OAuth users
                    CreatedAt = DateTime.UtcNow,
                    ProfileImage = googleUser.Picture,
                    EmailConfirmed = true, // Google emails are verified
                    IsActive = true,
                    Status = "active"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update profile image if available
                if (!string.IsNullOrEmpty(googleUser.Picture) && string.IsNullOrEmpty(user.ProfileImage))
                {
                    user.ProfileImage = googleUser.Picture;
                    await _context.SaveChangesAsync();
                }
            }

            // Block banned users
            if (user.IsBanned)
                return Unauthorized("Your account has been banned by admin");

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                id = user.Id,
                name = user.Name,
                phone = user.Phone,
                email = user.Email,
                profileImage = user.ProfileImage
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Google authentication failed: {ex.Message}");
        }
    }

    // ✅ Facebook OAuth Login/Signup
    [HttpPost("facebook")]
    [AllowAnonymous]
    public async Task<IActionResult> FacebookAuth([FromBody] FacebookAuthDto dto)
    {
        try
        {
            // Verify Facebook token and get user info
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={dto.AccessToken}");
            
            if (!response.IsSuccessStatusCode)
                return Unauthorized("Invalid Facebook token");

            var content = await response.Content.ReadAsStringAsync();
            var fbUser = System.Text.Json.JsonSerializer.Deserialize<FacebookUserInfo>(content);

            if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
                return BadRequest("Failed to get Facebook user info");

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == fbUser.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = fbUser.Name ?? "Facebook User",
                    Email = fbUser.Email,
                    Phone = "", // Facebook doesn't provide phone
                    PasswordHash = "", // No password for OAuth users
                    CreatedAt = DateTime.UtcNow,
                    ProfileImage = fbUser.Picture?.Data?.Url,
                    EmailConfirmed = true, // Facebook emails are verified
                    IsActive = true,
                    Status = "active"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update profile image if available
                if (fbUser.Picture?.Data?.Url != null && string.IsNullOrEmpty(user.ProfileImage))
                {
                    user.ProfileImage = fbUser.Picture.Data.Url;
                    await _context.SaveChangesAsync();
                }
            }

            // Block banned users
            if (user.IsBanned)
                return Unauthorized("Your account has been banned by admin");

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                id = user.Id,
                name = user.Name,
                phone = user.Phone,
                email = user.Email,
                profileImage = user.ProfileImage
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Facebook authentication failed: {ex.Message}");
        }
    }

    // ✅ Send Email OTP (manual trigger)
    [HttpPost("send-email-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmailOtp([FromBody] SendEmailOtpDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required");

        var existing = await _context.EmailOtps
            .Where(e => e.Email == dto.Email.Trim().ToLower() && e.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
            return BadRequest("OTP already sent. Please wait before requesting again.");

        var code = new Random().Next(1000, 9999).ToString();
        var otp = new EmailOtp
        {
            Email = dto.Email.Trim().ToLower(),
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(dto.Email, code);

        return Ok(new { message = "OTP sent to email" });
    }

    // ✅ Verify Email OTP
    [HttpPost("verify-email-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpDto dto)
    {
        var otp = await _context.EmailOtps
            .Where(e => e.Email == dto.Email.Trim().ToLower() && e.Code == dto.Code)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null || otp.ExpiresAt < DateTime.UtcNow)
            return BadRequest("Invalid or expired OTP");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();
        }

        _context.EmailOtps.Remove(otp);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Email verified successfully" });
    }

    // ✅ Forgot Password - Send OTP
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
        if (user == null)
            return BadRequest("Email not found");

        // Generate and send OTP
        var code = new Random().Next(1000, 9999).ToString();
        var otp = new EmailOtp
        {
            Email = dto.Email.Trim().ToLower(),
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };

        // Remove old OTPs for this email
        var oldOtps = await _context.EmailOtps
            .Where(e => e.Email == dto.Email.Trim().ToLower())
            .ToListAsync();
        _context.EmailOtps.RemoveRange(oldOtps);

        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(dto.Email, code);

        return Ok(new { message = "OTP sent to email" });
    }

    // ✅ Reset Password with OTP
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest("Email, code, and new password are required");

        // Verify OTP
        var otp = await _context.EmailOtps
            .Where(e => e.Email == dto.Email.Trim().ToLower() && e.Code == dto.Code)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null || otp.ExpiresAt < DateTime.UtcNow)
            return BadRequest("Invalid or expired OTP");

        // Update password
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
        if (user == null)
            return NotFound("User not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        
        // Remove used OTP
        _context.EmailOtps.Remove(otp);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully" });
    }

    // ✅ Get Profile
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            phone = user.Phone,
            email = user.Email,
            profileImage = user.ProfileImage,
            darkMode = user.DarkMode,
            notificationsEnabled = user.NotificationsEnabled,
            language = user.Language,
            hideProfile = user.HideProfile,
            allowMessages = user.AllowMessages,
            emailConfirmed = user.EmailConfirmed,
            verificationTier = user.VerificationTier,
            coinsBalance = user.CoinsBalance,
            createdAt = user.CreatedAt
        });
    }

    // ✅ Update Profile
    [HttpPut("update-profile")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(dto.Name))
            user.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Email))
            user.Email = dto.Email;

        if (dto.Image != null && dto.Image.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads/profiles");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.Image.CopyToAsync(stream);

            user.ProfileImage = $"/uploads/profiles/{fileName}";
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            profileImage = user.ProfileImage
        });
    }

    // ✅ Update Settings
    [HttpPut("update-settings")]
    [Authorize]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.DarkMode = dto.DarkMode;
        user.NotificationsEnabled = dto.NotificationsEnabled;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Settings updated" });
    }

    // ✅ Change Password
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    // ✅ Update Privacy
    [HttpPut("update-privacy")]
    [Authorize]
    public async Task<IActionResult> UpdatePrivacy([FromBody] UpdatePrivacyDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.HideProfile = dto.HideProfile;
        user.AllowMessages = dto.AllowMessages;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Privacy settings updated" });
    }

    // ✅ Update Language
    [HttpPut("update-language")]
    [Authorize]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Language = dto.Language;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Language updated" });
    }

    // ✅ Generate JWT Token
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "default-secret-key-min-32-chars"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("phone", user.Phone)
        };

        var expireMinutes = int.TryParse(jwtSettings["ExpireMinutes"], out var exp) ? exp : 60;

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "MeGo",
            audience: jwtSettings["Audience"] ?? "MeGoUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ✅ DTOs
public class UserSignupDto
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserLoginDto
{
    public string PhoneOrEmail { get; set; } = "";
    public string Password { get; set; } = "";
}

public class SendEmailOtpDto
{
    public string Email { get; set; } = "";
}

public class VerifyEmailOtpDto
{
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";
}

public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public IFormFile? Image { get; set; }
}

public class UpdateSettingsDto
{
    public bool DarkMode { get; set; }
    public bool NotificationsEnabled { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class UpdatePrivacyDto
{
    public bool HideProfile { get; set; }
    public bool AllowMessages { get; set; }
}

public class UpdateLanguageDto
{
    public string Language { get; set; } = "en";
}

public class GoogleAuthDto
{
    public string AccessToken { get; set; } = "";
}

public class FacebookAuthDto
{
    public string AccessToken { get; set; } = "";
}

public class GoogleUserInfo
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
}

public class FacebookUserInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public FacebookPicture? Picture { get; set; }
}

public class FacebookPicture
{
    public FacebookPictureData? Data { get; set; }
}

public class FacebookPictureData
{
    public string? Url { get; set; }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = "";
}

public class ResetPasswordDto
{
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
