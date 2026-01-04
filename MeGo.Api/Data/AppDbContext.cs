using Microsoft.EntityFrameworkCore;
using MeGo.Api.Models;

namespace MeGo.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ✅ DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<SpinHistory> SpinHistory { get; set; }
        public DbSet<TaskHistory> TaskHistory { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Listing> Listings { get; set; }   // if available
        public DbSet<Report> Reports { get; set; } 
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<KycInfo> KycInfos { get; set; }
        
        // New feature DbSets
        public DbSet<AdQualityScore> AdQualityScores { get; set; }
        public DbSet<AdHistory> AdHistories { get; set; }
        public DbSet<RecentlyViewed> RecentlyViewed { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }
        public DbSet<BuyerRequest> BuyerRequests { get; set; }
        public DbSet<BuyerRequestResponse> BuyerRequestResponses { get; set; }
        public DbSet<SellerRating> SellerRatings { get; set; }
        public DbSet<ChatReaction> ChatReactions { get; set; }
        public DbSet<AdAnalytics> AdAnalytics { get; set; }
        public DbSet<SellerTip> SellerTips { get; set; }
        public DbSet<BoostReferral> BoostReferrals { get; set; }
        public DbSet<AdShareTemplate> AdShareTemplates { get; set; }
        public DbSet<BuyerAlert> BuyerAlerts { get; set; }
        public DbSet<AdRelistReminder> AdRelistReminders { get; set; }
        public DbSet<PointsExchange> PointsExchanges { get; set; }
        public DbSet<MiniGame> MiniGames { get; set; }
        public DbSet<GamePlayHistory> GamePlayHistories { get; set; }
        
        // Orders and Billing
        public DbSet<Order> Orders { get; set; }
        public DbSet<BillingInfo> BillingInfos { get; set; }
        public DbSet<DeliveryOrder> DeliveryOrders { get; set; }
        public DbSet<DiscountedPackage> DiscountedPackages { get; set; }
        public DbSet<UserPackagePurchase> UserPackagePurchases { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // -----------------------------
    // FAVORITES RELATION
    // -----------------------------
    modelBuilder.Entity<Favorite>()
        .HasOne(f => f.Ad)
        .WithMany(a => a.Favorites)
        .HasForeignKey(f => f.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Favorite>()
        .HasOne(f => f.User)
        .WithMany(u => u.Favorites)
        .HasForeignKey(f => f.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // -----------------------------
    // MEDIA RELATION
    // -----------------------------
    modelBuilder.Entity<Media>()
        .HasOne(m => m.Ad)
        .WithMany(a => a.Media)
        .HasForeignKey(m => m.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    // -----------------------------
    // REPORTS RELATION
    // -----------------------------
    modelBuilder.Entity<Report>()
        .HasOne(r => r.Ad)
        .WithMany(a => a.Reports)
        .HasForeignKey(r => r.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Report>()
        .HasOne(r => r.User)
        .WithMany(u => u.Reports)
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // -----------------------------
    // MESSAGES RELATION
    // -----------------------------
    modelBuilder.Entity<Message>()
        .HasOne(m => m.Conversation)
        .WithMany(c => c.Messages)
        .HasForeignKey(m => m.ConversationId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<KycInfo>()
        .HasOne(k => k.User)
        .WithOne(u => u.KycInfo)
        .HasForeignKey<KycInfo>(k => k.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // SupportRequest relationship
    modelBuilder.Entity<SupportRequest>()
        .HasOne(s => s.User)
        .WithMany()
        .HasForeignKey(s => s.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // New feature relationships
    modelBuilder.Entity<AdQualityScore>()
        .HasOne(a => a.Ad)
        .WithMany()
        .HasForeignKey(a => a.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<AdHistory>()
        .HasOne(a => a.Ad)
        .WithMany()
        .HasForeignKey(a => a.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RecentlyViewed>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RecentlyViewed>()
        .HasOne(r => r.Ad)
        .WithMany()
        .HasForeignKey(r => r.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<SwapRequest>()
        .HasOne(s => s.Requester)
        .WithMany()
        .HasForeignKey(s => s.RequesterId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<BuyerRequest>()
        .HasOne(b => b.Buyer)
        .WithMany()
        .HasForeignKey(b => b.BuyerId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<BuyerRequestResponse>()
        .HasOne(b => b.BuyerRequest)
        .WithMany(br => br.Responses)
        .HasForeignKey(b => b.BuyerRequestId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<SellerRating>()
        .HasOne(s => s.Rater)
        .WithMany()
        .HasForeignKey(s => s.RaterId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<ChatReaction>()
        .HasOne(c => c.Message)
        .WithMany()
        .HasForeignKey(c => c.MessageId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<AdAnalytics>()
        .HasOne(a => a.Ad)
        .WithMany()
        .HasForeignKey(a => a.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<SellerTip>()
        .HasOne(s => s.Ad)
        .WithMany()
        .HasForeignKey(s => s.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<BoostReferral>()
        .HasOne(b => b.Ad)
        .WithMany()
        .HasForeignKey(b => b.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<BuyerAlert>()
        .HasOne(b => b.User)
        .WithMany()
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<AdRelistReminder>()
        .HasOne(a => a.Ad)
        .WithMany()
        .HasForeignKey(a => a.AdId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<PointsExchange>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<GamePlayHistory>()
        .HasOne(g => g.User)
        .WithMany()
        .HasForeignKey(g => g.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<GamePlayHistory>()
        .HasOne(g => g.Game)
        .WithMany()
        .HasForeignKey(g => g.GameId)
        .OnDelete(DeleteBehavior.Restrict);

    // Orders relationships
    modelBuilder.Entity<Order>()
        .HasOne(o => o.User)
        .WithMany()
        .HasForeignKey(o => o.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Order>()
        .HasOne(o => o.Ad)
        .WithMany()
        .HasForeignKey(o => o.AdId)
        .OnDelete(DeleteBehavior.Restrict);

    // BillingInfo relationships
    modelBuilder.Entity<BillingInfo>()
        .HasOne(b => b.User)
        .WithMany()
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // DeliveryOrder relationships
    modelBuilder.Entity<DeliveryOrder>()
        .HasOne(d => d.User)
        .WithMany()
        .HasForeignKey(d => d.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<DeliveryOrder>()
        .HasOne(d => d.Order)
        .WithMany()
        .HasForeignKey(d => d.OrderId_FK)
        .OnDelete(DeleteBehavior.Restrict);

    // DiscountedPackage relationships
    modelBuilder.Entity<UserPackagePurchase>()
        .HasOne(u => u.User)
        .WithMany()
        .HasForeignKey(u => u.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<UserPackagePurchase>()
        .HasOne(u => u.Package)
        .WithMany()
        .HasForeignKey(u => u.PackageId)
        .OnDelete(DeleteBehavior.Restrict);

}




    }
}
