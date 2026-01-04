using Microsoft.AspNetCore.Http;

namespace MeGo.Api.DTOs
{
    public class KycSubmissionDto
    {
        public string CnicNumber { get; set; } = string.Empty;
        public IFormFile? CnicFrontImage { get; set; }
        public IFormFile? CnicBackImage { get; set; }
        public IFormFile? Selfie { get; set; }
        public string VerificationTier { get; set; } = "Intermediate";
    }
}

