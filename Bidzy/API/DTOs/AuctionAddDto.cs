using Bidzy.Domain.Enties;

namespace Bidzy.API.Dto
{
    public class AuctionAddDto
    {
        public Guid ProductId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
    }
}
