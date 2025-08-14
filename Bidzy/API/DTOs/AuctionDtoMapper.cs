using Bidzy.API.DTOs.auctionDtos;
using Bidzy.Domain.Enties;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs
{
    public static class AuctionDtoMapper
    {
        public static AuctionReadDto ToReadDto(this Auction auction)
        {
            return new AuctionReadDto
            {
                Id = auction.Id,
                ProductId = auction.ProductId,
                ProductTitle = auction.Product?.Title,
                SellerId = auction.Product.SellerId,
                SellerName = auction.Product.Seller?.FullName,
                ImageUrl = auction.Product.ImageUrl,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                MinimumBid = auction.MinimumBid,
                Status = auction.Status.ToString(),
                WinningBidId = auction.WinningBidId,
                WinningBid = auction.WinningBid != null
                            ? $"{auction.WinningBid.Bidder?.FullName ?? "Unknown"} - {auction.WinningBid.Amount:C}"
                            : "No winning bid"
            };
        }

        public static Auction ToEntity (this AuctionAddDto auctionAddDto)
        {
            return new Auction
            {
                Id = Guid.NewGuid(),
                ProductId = auctionAddDto.ProductId,
                StartTime = auctionAddDto.StartTime,
                EndTime = auctionAddDto.EndTime,
                MinimumBid = auctionAddDto.MinimumBid,
                Status = Domain.Enum.AuctionStatus.Scheduled
            };
        }

        public static void UpdateEntity (this Auction auction, AuctionUpdateDto auctionUpdateDto)
        {
            if (auctionUpdateDto.StartTime.HasValue)
            {
                auction.StartTime = auctionUpdateDto.StartTime.Value;
            }
            if (auctionUpdateDto.EndTime.HasValue)
            {
                auction.EndTime = auctionUpdateDto.EndTime.Value;
            }
            if (auctionUpdateDto.MinimumBid.HasValue)
            {
                auction.MinimumBid = auctionUpdateDto.MinimumBid.Value;
            }
            if (!string.IsNullOrWhiteSpace(auctionUpdateDto.Status) && Enum.TryParse(auctionUpdateDto.Status, true, out AuctionStatus status))
            {
                auction.Status = status;
            }
            if (auctionUpdateDto.WinnerId.HasValue)
            {
                //auction.WinnerId = auctionUpdateDto.WinnerId;
            }
        } 
    }
}
