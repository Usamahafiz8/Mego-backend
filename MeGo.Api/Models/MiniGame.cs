using System;

namespace MeGo.Api.Models
{
    public class MiniGame
    {
        public int Id { get; set; }
        public string GameName { get; set; } = "";
        public string GameType { get; set; } = ""; // "tap", "match", "puzzle"
        public string? GameConfig { get; set; } // JSON config
        public bool IsActive { get; set; } = true;
        public int CoinsPerPlay { get; set; } = 0; // Cost to play
        public int MinCoinsWon { get; set; } = 0;
        public int MaxCoinsWon { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class GamePlayHistory
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int GameId { get; set; }
        public MiniGame Game { get; set; } = null!;
        
        public int CoinsSpent { get; set; }
        public int CoinsWon { get; set; }
        public int Score { get; set; }
        
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}

