namespace Bidzy.Application.Repository.Interfaces
{
    public interface INotificationSchedulerService
    {
        void ScheduleAuctionStartEmail(string auctionId, string receiverEmail, DateTime startTime);
        void ScheduleAuctionEndEmail(string auctionId, string receiverEmail, string winnerName, DateTime endTime);
        void ScheduleRecurringSummaryEmail(string receiverEmail);
    }
}
