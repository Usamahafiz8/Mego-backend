using MeGo.Api.Data;
using MeGo.Api.Models;

namespace MeGo.Api.Services
{
    public class RewardService
    {
        private readonly AppDbContext _context;

        public RewardService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Add coins to a user
        public async Task AddCoinsAsync(Guid userId, int amount, string reason = "")
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.CoinsBalance += amount;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[REWARD] User {userId} earned {amount} coins for: {reason}");
        }

        // ✅ Give reward (alias for AddCoinsAsync for consistency)
        public async Task GiveRewardAsync(Guid userId, string rewardType, int amount)
        {
            await AddCoinsAsync(userId, amount, rewardType);
        }
    }
}
