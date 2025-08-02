using Bidzy.Domain.Enties;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Data
{
    public class ApplicationDbContext :DbContext
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

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();



        }

    }
}
