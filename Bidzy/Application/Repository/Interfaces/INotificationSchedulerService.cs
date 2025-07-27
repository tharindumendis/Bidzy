namespace Bidzy.Application.Repository.Interfaces
{
    public interface INotificationSchedulerService
    {
        void ScheduleAuctionStartEmail(string auctionId, string receiverEmail, DateTime startTime);
        void ScheduleAuctionEndEmail(string auctionId, string receiverEmail, string winnerName, TimeSpan delay);
        void ScheduleRecurringSummaryEmail(string receiverEmail);
    }
}
