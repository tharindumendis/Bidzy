using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Entities
{
    public class AuctionParticipation
    {
        public required User User { get; set; }
        public required Auction Auction { get; set; }
        public Guid userId { get; set; }
        public Guid auctionId { get; set; }
        public ParticipationStatus status { get; set; }

    }
}
