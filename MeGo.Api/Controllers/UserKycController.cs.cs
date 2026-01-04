// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using MeGo.Api.Data;
// using MeGo.Api.Models;

// namespace MeGo.Api.Controllers
// {
//     [ApiController]
//     [Route("v1/kyc")]
//     [Authorize]
//     public class UserKycController : ControllerBase
//     {
//         private readonly AppDbContext _context;
//         private readonly IWebHostEnvironment _env;

//         public UserKycController(AppDbContext context, IWebHostEnvironment env)
//         {
//             _context = context;
//             _env = env;
//         }

//         // ⭐ GET KYC STATUS (FOR LOGGED-IN USER)
//         [HttpGet("status")]
//         public async Task<IActionResult> GetMyKycStatus()
//         {
//             var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
//             if (idClaim == null)
//                 return Unauthorized(new { error = "User ID missing in token" });

//             var userId = Guid.Parse(idClaim.Value);

//             var kyc = await _context.KycInfos
//                 .FirstOrDefaultAsync(k => k.UserId == userId);

//             if (kyc == null)
//                 return Ok(new { status = "NotSubmitted" });

//             return Ok(new
//             {
//                 status = kyc.Status,
//                 rejectionReason = kyc.RejectionReason,
//                 cnic = kyc.CnicNumber,
//                 front = kyc.CnicFrontImageUrl,
//                 back = kyc.CnicBackImageUrl,
//                 selfie = kyc.SelfieUrl
//             });
//         }

//         // ⭐ SAVE IMAGE METHOD
//         private async Task<string> SaveImage(IFormFile file)
//         {
//             if (file == null) return null;

//             string folder = Path.Combine(_env.WebRootPath, "kyc");
//             if (!Directory.Exists(folder))
//                 Directory.CreateDirectory(folder);

//             string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
//             string filePath = Path.Combine(folder, fileName);

//             using (var stream = new FileStream(filePath, FileMode.Create))
//             {
//                 await file.CopyToAsync(stream);
//             }

//             return "/kyc/" + fileName;
//         }

//         // ⭐ SUBMIT KYC (FORMDATA SUPPORTED)
//         [HttpPost("submit")]
//         public async Task<IActionResult> SubmitKyc(
//             [FromForm] string CnicNumber,
//             [FromForm] IFormFile CnicFrontImage,
//             [FromForm] IFormFile CnicBackImage,
//             [FromForm] IFormFile Selfie
//         )
//         {
//             var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
//             if (idClaim == null)
//                 return Unauthorized(new { error = "User ID missing in token" });

//             var userId = Guid.Parse(idClaim.Value);

//             var user = await _context.Users.FindAsync(userId);
//             if (user == null)
//                 return NotFound(new { message = "User not found" });

//             // Upload Images
//             string frontUrl = await SaveImage(CnicFrontImage);
//             string backUrl = await SaveImage(CnicBackImage);
//             string selfieUrl = await SaveImage(Selfie);

//             var existing = await _context.KycInfos.FirstOrDefaultAsync(k => k.UserId == userId);

//             if (existing != null)
//             {
//                 existing.CnicNumber = CnicNumber;
//                 existing.CnicFrontImageUrl = frontUrl;
//                 existing.CnicBackImageUrl = backUrl;
//                 existing.SelfieUrl = selfieUrl;
//                 existing.Status = "Pending";
//                 existing.RejectionReason = null;
//                 existing.ReviewedAt = null;
//             }
//             else
//             {
//                 _context.KycInfos.Add(new KycInfo
//                 {
//                     UserId = userId,
//                     CnicNumber = CnicNumber,
//                     CnicFrontImageUrl = frontUrl,
//                     CnicBackImageUrl = backUrl,
//                     SelfieUrl = selfieUrl,
//                     Status = "Pending",
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }

//             await _context.SaveChangesAsync();
//             return Ok(new { message = "KYC submitted successfully!" });
//         }
//     }
// }
