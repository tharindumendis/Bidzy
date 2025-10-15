using Bidzy.Application.DTOs;
using Bidzy.Application.Services.Email;
using Bidzy.Application.Services.Scheduler;
using Bidzy.Domain.Entities;
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

        public void ScheduleAuctionStartEmail(Auction auction, List<string> emailAddresses, DateTime startTime)
        {
            
            var delay = startTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IEmailJobService>(
                service => service.SendAuctionStartedEmailsAsync(auction, emailAddresses),
                delay);
            }
            else
            {

                _emailJobService.SendAuctionStartedEmailsAsync(auction, emailAddresses);

            }
        }

        public void ScheduleAuctionEndEmail(Auction auction,Domain.Entities.Bid bid, DateTime endTime)
        {
            var delay = endTime - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                _jobScheduler.Schedule<IEmailJobService>(
                service => service.SendAuctionEndedEmails(auction,bid),
                delay);
            }
            else
            {

                _emailJobService.SendAuctionEndedEmails(auction, bid);

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
