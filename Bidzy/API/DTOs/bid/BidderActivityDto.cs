using Bidzy.API.DTOs.auctionDtos;

namespace Bidzy.API.DTOs.bidDtos
{
    public class BidderActivityDto
    {
        public IEnumerable<BidderAuctionDto> ParticipatedAuctions { get; set; }
        public IEnumerable<BidderAuctionDto> WonAuctions { get; set; }
    }
}
