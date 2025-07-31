using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface INotificationSchedulerService
    {
        void ScheduleAuctionStartEmail(Auction auction, List<string> emailAddresses, DateTime startTime);
        void ScheduleAuctionEndEmail(Auction auction, Bid bid, DateTime endTime);
        void ScheduleRecurringSummaryEmail(string receiverEmail);
    }
}
