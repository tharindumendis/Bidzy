using Bidzy.Domain.Entities;

namespace Bidzy.Application.Services.NotificationSchedulerService
{
    public interface INotificationSchedulerService
    {
        void ScheduleAuctionStartEmail(Auction auction, List<string> emailAddresses, DateTime startTime);
        void ScheduleAuctionEndEmail(Auction auction, Domain.Entities.Bid bid, DateTime endTime);
        void ScheduleRecurringSummaryEmail(string receiverEmail);
    }
}
