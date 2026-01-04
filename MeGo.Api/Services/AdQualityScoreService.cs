using MeGo.Api.Data;
using MeGo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeGo.Api.Services
{
    public class AdQualityScoreService
    {
        private readonly AppDbContext _context;

        public AdQualityScoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CalculateAndSaveQualityScore(int adId)
        {
            var ad = await _context.Ads
                .Include(a => a.Media)
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad == null) return;

            // Calculate individual scores
            int titleScore = CalculateTitleScore(ad.Title);
            int imageScore = CalculateImageScore(ad.Media?.Count ?? 0);
            int descriptionScore = CalculateDescriptionScore(ad.Description);
            int completenessScore = CalculateCompletenessScore(ad);

            // Calculate overall score (weighted average)
            int overallScore = (int)(
                (titleScore * 0.25) +
                (imageScore * 0.30) +
                (descriptionScore * 0.25) +
                (completenessScore * 0.20)
            );

            // Save or update quality score
            var existingScore = await _context.AdQualityScores
                .FirstOrDefaultAsync(s => s.AdId == adId);

            if (existingScore != null)
            {
                existingScore.TitleScore = titleScore;
                existingScore.ImageScore = imageScore;
                existingScore.DescriptionScore = descriptionScore;
                existingScore.CompletenessScore = completenessScore;
                existingScore.OverallScore = overallScore;
                existingScore.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                var newScore = new AdQualityScore
                {
                    AdId = adId,
                    TitleScore = titleScore,
                    ImageScore = imageScore,
                    DescriptionScore = descriptionScore,
                    CompletenessScore = completenessScore,
                    OverallScore = overallScore,
                    CalculatedAt = DateTime.UtcNow
                };
                _context.AdQualityScores.Add(newScore);
            }

            await _context.SaveChangesAsync();
        }

        private int CalculateTitleScore(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return 0;

            int score = 0;
            int length = title.Length;

            // Length score (optimal: 30-60 chars)
            if (length >= 10 && length <= 100) score += 40;
            else if (length > 100) score += 20;
            else score += 10;

            // Keyword quality (check for common spam words)
            var spamWords = new[] { "urgent", "hurry", "limited", "click here", "free" };
            bool hasSpam = spamWords.Any(word => title.ToLower().Contains(word));
            if (!hasSpam) score += 30;

            // Capitalization (proper case is better)
            if (title == System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()))
                score += 30;
            else if (!title.All(char.IsUpper)) score += 20;

            return Math.Min(100, score);
        }

        private int CalculateImageScore(int imageCount)
        {
            if (imageCount == 0) return 0;
            if (imageCount >= 5) return 100;
            if (imageCount >= 3) return 80;
            if (imageCount >= 2) return 60;
            return 40; // 1 image
        }

        private int CalculateDescriptionScore(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return 0;

            int score = 0;
            int length = description.Length;

            // Length score (optimal: 100-500 chars)
            if (length >= 100 && length <= 1000) score += 50;
            else if (length >= 50) score += 30;
            else score += 10;

            // Check for detailed information
            var detailKeywords = new[] { "condition", "specification", "warranty", "location", "contact" };
            int keywordCount = detailKeywords.Count(kw => description.ToLower().Contains(kw));
            score += Math.Min(50, keywordCount * 10);

            return Math.Min(100, score);
        }

        private int CalculateCompletenessScore(Ad ad)
        {
            int score = 0;
            int fieldsFilled = 0;
            int totalFields = 8;

            if (!string.IsNullOrWhiteSpace(ad.Title)) fieldsFilled++;
            if (!string.IsNullOrWhiteSpace(ad.Description)) fieldsFilled++;
            if (ad.Price > 0) fieldsFilled++;
            if (!string.IsNullOrWhiteSpace(ad.Category)) fieldsFilled++;
            if (!string.IsNullOrWhiteSpace(ad.Location)) fieldsFilled++;
            if (!string.IsNullOrWhiteSpace(ad.Contact)) fieldsFilled++;
            if (!string.IsNullOrWhiteSpace(ad.Condition)) fieldsFilled++;
            if (ad.Media != null && ad.Media.Count > 0) fieldsFilled++;

            score = (fieldsFilled * 100) / totalFields;
            return score;
        }
    }
}

