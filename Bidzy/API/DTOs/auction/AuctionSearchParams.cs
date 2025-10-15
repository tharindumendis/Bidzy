using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.auctionDtos
{
    public class AuctionSearchParams
    {
        public string? Title { get; set; }
        public AuctionStatus? Status { get; set; }
        public AuctionCategories? Category { get; set; }
        public Guid? SellerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
