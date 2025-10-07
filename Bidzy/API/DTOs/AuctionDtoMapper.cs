using Bidzy.API.DTOs.auctionDtos;
using Bidzy.API.DTOs.bidDtos;
using Bidzy.Application.DTOs;
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
                Description = auction.Product?.Description,
                SellerId = auction.Product.SellerId,
                SellerName = auction.Product.Seller?.FullName,
                ImageUrl = auction.Product.ImageUrl,
                tags = auction.Product.Tags.Select(t => t.tagName).ToArray(),
                Category = auction.Category.ToString(),
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                MinimumBid = auction.MinimumBid,
                Status = auction.Status.ToString(),
                WinningBidId = auction.WinningBidId,
                WinAmount = auction.WinningBid != null ? auction.WinningBid.Amount : null,
                WinnerId = auction.WinningBid?.BidderId != null ? auction.WinningBid?.BidderId : null
            };
        }
        public static AuctionReadDto ToReadDto(this AuctionWithMaxBidDto auctionMaxDto)
        {
            AuctionReadDto auctionReadDto = new AuctionReadDto
            {
                Id = auctionMaxDto.Auction.Id,
                ProductId = auctionMaxDto.Auction.ProductId,
                ProductTitle = auctionMaxDto.Auction.Product?.Title,
                Description = auctionMaxDto.Auction.Product?.Description,
                SellerId = auctionMaxDto.Auction.Product.SellerId,
                SellerName = auctionMaxDto.Auction.Product.Seller?.FullName,
                ImageUrl = auctionMaxDto.Auction.Product.ImageUrl,
                tags = auctionMaxDto.Auction.Product.Tags.Select(t => t.tagName).ToArray(),
                Category = auctionMaxDto.Auction.Category.ToString(),
                StartTime = auctionMaxDto.Auction.StartTime,
                EndTime = auctionMaxDto.Auction.EndTime,
                MinimumBid = auctionMaxDto.Auction.MinimumBid,
                Status = auctionMaxDto.Auction.Status.ToString(),
                WinningBidId = auctionMaxDto.Auction.WinningBidId,
                WinAmount = auctionMaxDto.Auction.WinningBid != null ? auctionMaxDto.Auction.WinningBid.Amount : null,
                WinnerId = auctionMaxDto.Auction.WinningBid?.BidderId != null ? auctionMaxDto.Auction.WinningBid?.BidderId : null
            };
            if (auctionMaxDto.MaxBidAmount.HasValue)
            {
                auctionReadDto.MaxBid = auctionMaxDto.MaxBidAmount.Value;
            }
            return auctionReadDto;
        }
        public static Auction ToEntity (this AuctionAddDto auctionAddDto)
        {
            return new Auction
            {
                Id = Guid.NewGuid(),
                ProductId = auctionAddDto.ProductId,
                StartTime = auctionAddDto.StartTime.ToUniversalTime(),
                EndTime = auctionAddDto.EndTime.ToUniversalTime(),
                MinimumBid = auctionAddDto.MinimumBid,
                Status = Domain.Enum.AuctionStatus.Scheduled,
                Category = auctionAddDto.Category
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
            if (auctionUpdateDto.Category.HasValue)
            {
                auction.Category = auctionUpdateDto.Category.Value;
            }
        }

        public static ShopAuctionDto ToshopAuctionDto(this Auction auction )
        {
            return new ShopAuctionDto
            {
                Id = auction.Id,
                ProductId = auction.ProductId,
                ProductTitle = auction.Product.Title,
                tags = auction.Product.Tags.Select(t => t.tagName).ToArray(),
                ImageUrl = auction.Product.ImageUrl,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                MinimumBid = auction.MinimumBid,
                Status = auction.Status.ToString(),
                Category = auction.Category.ToString(),
                WinBid = auction.WinningBid?.ToReadDto(),
                Bids = auction.Bids.Select(b => b.ToReadDto()).ToList() ?? new List<BidReadDto>(),
                ViewCount = auction.ViewHistories?.Count ?? 0,
                LikeCount = auction.LikedByUsers?.Count ?? 0,
                ParticipationCount = auction.participations?.Count ?? 0
            };
        }
    }
}
