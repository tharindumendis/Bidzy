using System.ComponentModel.DataAnnotations.Schema;

namespace Bidzy.Domain.Enties
{
    public class UserAuctionFavorite
    {
        public Guid userId { get; set; }
        public User user { get; set; }

        public Guid auctionId { get; set; }
        public Auction auction { get; set; }
        public DateTime likedAt { get; set; } = DateTime.UtcNow;
    }
}
