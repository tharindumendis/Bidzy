using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestJobController : ControllerBase
    {
        private readonly INotificationSchedulerService _notificationSchedulerService;

        public TestJobController(INotificationSchedulerService notificationSchedulerService)
        {
            _notificationSchedulerService = notificationSchedulerService;
        }

        [HttpPost("auction-start")]
        public IActionResult ScheduleAuctionStartEmail([FromQuery] string auctionId, [FromQuery] string email)
        {
            //_notificationSchedulerService.ScheduleAuctionStartEmail(auctionId, email, DateTime.Now);
            return Ok("Scheduled auction start email.");
        }
    }
}
