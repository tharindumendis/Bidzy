namespace Bidzy.API.DTOs.favoriteAuctionsDtos
{
    public class userAuctionFavoriteReadDto
    {
        public Guid auctionId { get; set; }
        public string productTitle { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
