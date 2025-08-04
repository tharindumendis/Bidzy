using Bidzy.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bidzy.Domain.Enties
{
    public class Auction
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        [Required]
        public Product Product { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public AuctionStatus Status { get; set; } // Scheduled, Active, Ended, Cancelled

        public Guid? WinningBidId { get; set; }
        public Bid? WinningBid { get; set; }

        [JsonIgnore]
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        [JsonIgnore]
        public ICollection<UserAuctionFavorite> LikedByUsers { get; set; } = new List<UserAuctionFavorite>();

    }
}
