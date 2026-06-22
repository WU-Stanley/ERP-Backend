using WUIAM.DTOs;

namespace WUIAM.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false, int limit = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task<NotificationDto> NotifyUserAsync(Guid userId, string title, string message, string type = "info", string? entityType = null, Guid? entityId = null);
        Task<List<NotificationDto>> NotifyAdminsAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null);
        Task<List<NotificationDto>> NotifyRecruitmentTeamAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null);
        Task<List<NotificationDto>> NotifyIctOnboardingTeamAsync(Guid applicationId, string employeeName, string position, DateTime? startDate = null);
    }
}
