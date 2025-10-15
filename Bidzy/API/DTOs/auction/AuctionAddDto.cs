using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;

namespace Bidzy.API.DTOs.auctionDtos
{
    public class AuctionAddDto
    {
        public Guid ProductId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
        public AuctionCategories Category { get; set; }
    }
}
