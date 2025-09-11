using Bidzy.API.DTOs.bidDtos;
using Bidzy.Domain.Enties;

namespace Bidzy.API.DTOs
{
    public static class BidDtoMapper
    {
        public static BidReadDto ToReadDto (this Bid bid)
        {
            return new BidReadDto
            {
                Id = bid.Id,
                AuctionId = bid.AuctionId,
                Title = bid.Auction?.Product?.Title,
                BidderId = bid.BidderId,
                BidderImageUrl = bid.Bidder?.imageUrl,
                BidderName = bid.Bidder?.FullName ?? string.Empty,
                Amount = bid.Amount,
                Timestamp = bid.Timestamp
            };
        }

        public static Bid ToEntity (this BidAddDto bidAddDto)
        {
            return new Bid
            {
                Id = Guid.NewGuid(),
                AuctionId = bidAddDto.AuctionId,
                BidderId = bidAddDto.BidderId,
                Amount = bidAddDto.Amount,
                Timestamp = DateTime.UtcNow
            };
        }

        public static void UpdateEntity (this Bid bid, BidUpdateDto bidUpdateDto)
        {
            if (bidUpdateDto.Amount.HasValue)
            {
                bid.Amount = bidUpdateDto.Amount.Value;
            }
        }
    }
}
