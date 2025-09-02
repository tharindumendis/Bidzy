using Bidzy.API.DTOs;
using Bidzy.API.DTOs.NotificationDtos;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bidzy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationController (INotificationRepository notificationRepository) {
            this.notificationRepository = notificationRepository;
        }

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> GetUserNotifications()
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var notifications = await notificationRepository.GetNotificationsByUserIdAsync(Guid.Parse(userId));
        //    var dtoList = notifications.Select(NotificationDtoMapper.ToDto);
        //    return Ok(dtoList);
        //}

        [Authorize]
        // if we get all notification add a path to this 
        [HttpGet]
        public async Task<IActionResult> GetUnseenNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = await notificationRepository.GetUnseenNotificationsByUserIdAsync(Guid.Parse(userId));
            var dtoList = notifications.Select(NotificationDtoMapper.ToDto);
            return Ok(dtoList);
        }

        [HttpPost]
        public async Task<IActionResult> AddNotifications([FromBody] CreateNotificationDto dto)
        {
            var entity = NotificationDtoMapper.ToEntity(dto);
            await notificationRepository.AddNotificationAsync(entity);
            return Ok(NotificationDtoMapper.ToDto(entity));
        }

        [HttpPut("mark-as-seen/{notificationId}")]
        public async Task<IActionResult> MarkAsSeen([FromRoute] Guid notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await notificationRepository.MarkAsSeenAsync(notificationId , Guid.Parse(userId));
            return NoContent();
        }

        [Authorize]
        [HttpPut("mark-all-as-seen/{userId}")]
        public async Task<IActionResult> MarkAllAsSeen([FromRoute] Guid userId)
        {
            await notificationRepository.MarkAllAsSeenByUserIdAsync(userId);
            return NoContent();
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] Guid notificationId)
        {
            await notificationRepository.DeleteNotificationAsync(notificationId);
            return NoContent();
        }
    }
}
