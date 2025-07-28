using Bidzy.Application.DTOs;
using Bidzy.Application.Repository.Interfaces;
using Hangfire;

namespace Bidzy.Application.Services.NotificationSchedulerService
{
    public class NotificationSchedulerService : INotificationSchedulerService
    {
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmailJobService _emailJobService;

        public NotificationSchedulerService(IJobScheduler jobScheduler, IEmailJobService emailJobService)
        {
            _jobScheduler = jobScheduler;
            _emailJobService = emailJobService;
        }

        public void ScheduleAuctionStartEmail(string auctionId, string receiverEmail, DateTime startTime)
        {
            var delay = startTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IEmailJobService>(
                service => service.SendAuctionStartedEmail(auctionId, receiverEmail),
                delay);
            }
            else
            {

                _emailJobService.SendAuctionStartedEmail(auctionId, receiverEmail);

            }
        }

        public void ScheduleAuctionEndEmail(string auctionId, string receiverEmail, string winnerName, DateTime endTime)
        {
            var delay = endTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IEmailJobService>(
                service => service.SendAuctionEndedEmail(auctionId, receiverEmail, winnerName),
                delay);
            }
            else
            {

                _emailJobService.SendAuctionEndedEmail(auctionId, receiverEmail, winnerName);

            }
        }

        public void ScheduleRecurringSummaryEmail(string receiverEmail)
        {
            
            _jobScheduler.Recurring<IEmailJobService>(
                $"daily-summary-{receiverEmail}",
                service => service.SendEmailAsync(new EmailDto
                {
                    ReceiverEmail = receiverEmail,
                    Subject = "Daily Summary",
                    Body = "Here's your daily auction summary."
                }),
                Cron.Daily());
       }
    }
}
