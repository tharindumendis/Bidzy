using Bidzy.Modles.Enties;

namespace Bidzy.Modles.Dto
{
    public class AuctionAddDto
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinimumBid { get; set; }
    }
}
