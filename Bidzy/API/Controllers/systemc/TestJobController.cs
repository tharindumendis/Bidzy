using Bidzy.API.Hubs;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Services.NotificationSchedulerService;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Entities;
using Bidzy.Domain.Enum;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers.systemc
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestJobController : ControllerBase
    {
        private readonly INotificationSchedulerService _notificationSchedulerService;
        private readonly ISignalRNotifier _signalRNotifier;
        private readonly IAuctionRepository _auctionRepository;
        

        public TestJobController(INotificationSchedulerService notificationSchedulerService, ISignalRNotifier signalRNotifier, IAuctionRepository auctionRepository)
        {
            _notificationSchedulerService = notificationSchedulerService;
            _signalRNotifier = signalRNotifier;
            _auctionRepository = auctionRepository;
        }

        [HttpPost("auction-start")]
        public IActionResult ScheduleAuctionStartEmail([FromQuery] string auctionId, [FromQuery] string email)
        {
            //_notificationSchedulerService.ScheduleAuctionStartEmail(auctionId, email, DateTime.Now);
            return Ok("Scheduled auction start email.");
        }
        [HttpGet]
        public async Task<IActionResult> Notify([FromQuery]string id)
        {
            Console.WriteLine(id);
              if (Guid.TryParse(id, out Guid auctionId))
            {
                var auc = await _auctionRepository.GetAuctionByIdAsync(auctionId);
                if (auc != null)
                {
                    await _signalRNotifier.BroadcastAuctionStarted(auc);
                }
                Domain.Entities.Notification notification = new Domain.Entities.Notification
                {
                    Id = Guid.Parse("f974b7cb-a12e-4b67-ae12-d3c079efa594"),
                    UserId = auctionId,
                    Message = $"The auction for  you favorited has been cancelled. Asynchronous methods enable EF ",
                    Type = NotificationType.AUCTIONSTART,
                    Link = "234-23432432-42323324",
                    IsSeen = false
                };
                await _signalRNotifier.SendNotificationToUser(notification);
            }


            return Ok(null);
        }
    }
}
