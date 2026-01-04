using MeGo.Api.Data;
using MeGo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Services
{
    public class SpamDetectionService
    {
        private readonly AppDbContext _context;
        private const int SPAM_THRESHOLD = 3; // Auto-hide after 3 spam reports
        private const int FRAUD_THRESHOLD = 2; // Auto-hide after 2 fraud reports

        public SpamDetectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CheckAndHandleSpamReports(int adId)
        {
            var ad = await _context.Ads
                .Include(a => a.Reports)
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad == null) return;

            // Count spam and fraud reports
            int spamCount = ad.Reports.Count(r => r.Reason.ToLower().Contains("spam"));
            int fraudCount = ad.Reports.Count(r => r.Reason.ToLower().Contains("fraud"));

            ad.SpamReportCount = spamCount;
            ad.FraudReportCount = fraudCount;

            // Auto-hide if threshold reached
            if (spamCount >= SPAM_THRESHOLD || fraudCount >= FRAUD_THRESHOLD)
            {
                ad.IsSpam = spamCount >= SPAM_THRESHOLD;
                ad.IsFraud = fraudCount >= FRAUD_THRESHOLD;
                ad.IsActive = false;
                ad.AutoHiddenAt = DateTime.UtcNow;
                ad.Status = "hidden"; // Add to manual review queue
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DetectKeywordSpamming(int adId)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return false;

            // Check for repeated keywords
            var words = ad.Title.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordCounts = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
            
            // If any word appears more than 3 times, it's likely spam
            return wordCounts.Any(w => w.Value > 3);
        }

        public async Task<bool> DetectDuplicateImages(int adId)
        {
            var ad = await _context.Ads
                .Include(a => a.Media)
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad?.Media == null || ad.Media.Count == 0) return false;

            // Check if same image URL appears in multiple ads by same user
            var imageUrls = ad.Media.Select(m => m.FilePath).ToList();
            var duplicateCount = await _context.Ads
                .Where(a => a.UserId == ad.UserId && a.Id != adId)
                .Include(a => a.Media)
                .CountAsync(a => a.Media.Any(m => imageUrls.Contains(m.FilePath)));

            return duplicateCount > 0;
        }
    }
}

