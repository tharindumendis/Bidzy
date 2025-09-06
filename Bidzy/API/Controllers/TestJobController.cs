using Bidzy.API.Hubs;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Enties;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
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
            }

            return Ok(null);
        }
    }
}
