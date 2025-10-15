using Bidzy.Domain.Entities;

namespace Bidzy.Application.DTOs
{
    public class AuctionWithMaxBidDto
    {
        public Auction Auction { get; set; }
        public decimal? MaxBidAmount { get; set; }
    }
}
