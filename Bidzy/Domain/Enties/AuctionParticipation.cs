using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class AuctionParticipation
    {
        public required User User { get; set; }
        public required Auction Auction { get; set; }
        public Guid userId { get; set; }
        public Guid auctionId { get; set; }
        public ParticipationStatus Status { get; set; }

    }
}
