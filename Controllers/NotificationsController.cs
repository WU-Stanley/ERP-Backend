using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Hubs;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int limit = 20)
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly, limit);
            return Ok(ApiResponse<List<NotificationDto>>.Success("Notifications retrieved successfully", notifications));
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<object>>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<object>.Success("Unread notification count retrieved successfully", count));
        }

        [HttpPut("{id:guid}/read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationRead", id);
            return Ok(ApiResponse<object>.Success("Notification marked as read", null));
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationsReadAll");
            return Ok(ApiResponse<object>.Success("All notifications marked as read", null));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User identity is missing.");
            }

            return userId;
        }
    }
}
