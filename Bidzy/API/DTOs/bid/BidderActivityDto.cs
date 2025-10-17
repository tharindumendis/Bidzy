using Bidzy.API.DTOs.auction;

namespace Bidzy.API.DTOs.bid
{
    public class BidderActivityDto
    {
        public IEnumerable<BidderAuctionDto> ParticipatedAuctions { get; set; }
        public IEnumerable<BidderAuctionDto> WonAuctions { get; set; }
    }
}
