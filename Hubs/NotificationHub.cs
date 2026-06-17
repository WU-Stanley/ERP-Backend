using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WUIAM.Interfaces;

namespace WUIAM.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notificationService;

        public NotificationHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<int> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            return await _notificationService.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsRead(Guid notificationId)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(notificationId, userId);
            await Clients.Caller.SendAsync("NotificationRead", notificationId);
        }

        public async Task MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            await Clients.Caller.SendAsync("NotificationsReadAll");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new HubException("User identity is missing.");
            }

            return userId;
        }
    }
}
