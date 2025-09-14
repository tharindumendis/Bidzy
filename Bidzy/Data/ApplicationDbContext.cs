using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Tag> Tags { get; set; } 
        public DbSet<UserAuctionFavorite> UserAuctionFavorite { get; set; }
        public DbSet<AuctionParticipation> AuctionParticipations { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<ViewHistory> ViewHistories { get; set; }

        public DbSet<WebhookEventLog> WebhookEventLogs { get; set; }
        public DbSet<Otp> Otps { get; set; }

        public DbSet<AppReview> AppReviews { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);
            // Prevents multiple cascade paths

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents cascade path conflict


            modelBuilder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.WinningBid)
                .WithMany()
                .HasForeignKey(a => a.WinningBidId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Auction>()
                .Property(a => a.MinimumBid)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Commission)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountCaptured)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.ProcessorFee)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.NetAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.RefundAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentIntentId)
                .IsUnique()
                .HasFilter("[PaymentIntentId] IS NOT NULL");

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ChargeId)
                .IsUnique()
                .HasFilter("[ChargeId] IS NOT NULL");

            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.Status, p.PaidAt });

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.BidId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.RefundId)
                .IsUnique()
                .HasFilter("[RefundId] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserAuctionFavorite>()
                .HasKey(ual => new { ual.userId, ual.auctionId });

            modelBuilder.Entity<UserAuctionFavorite>()
                .HasOne(ual => ual.user)
                .WithMany(u => u.AuctionLikes)
                .HasForeignKey(ual => ual.userId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAuctionFavorite>()
                .HasOne(ual => ual.auction)
                .WithMany(a => a.LikedByUsers)
                .HasForeignKey(ual => ual.auctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuctionParticipation>()
                .HasKey(ap => new { ap.userId, ap.auctionId });

            modelBuilder.Entity<AuctionParticipation>()
                .HasOne(ap => ap.User)
                .WithMany(a => a.AuctionParticipations)
                .HasForeignKey(ap => ap.userId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AuctionParticipation>()
                .HasOne(ap => ap.Auction)
                .WithMany( a => a.participations)
                .HasForeignKey(ap=> ap.auctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewHistory>()
                .HasOne(vh => vh.User)
                .WithMany(u => u.ViewHistories)
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViewHistory>()
                .HasOne(vh => vh.Auction)
                .WithMany(a => a.ViewHistories)
                .HasForeignKey(vh => vh.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Auction>()
                .Property(a => a.WinAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AppReview>()
                .HasKey(a => a.Id);

        }

    }
}
