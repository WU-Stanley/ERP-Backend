using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Hubs;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Services
{
    public class NotificationService : INotificationService
    {
        private readonly WUIAMDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(WUIAMDbContext context, IHttpContextAccessor httpContextAccessor, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false, int limit = 20)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .Select(MapToDto)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<NotificationDto> NotifyUserAsync(Guid userId, string title, string message, string type = "info", string? entityType = null, Guid? entityId = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                EntityType = entityType,
                EntityId = entityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Push real-time notification via SignalR
            var clientDto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("NewNotification", clientDto);

            return dto;
        }

        public async Task<List<NotificationDto>> NotifyAdminsAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null)
        {
            var userIds = await GetAdminUserIdsAsync();
            var notifications = new List<NotificationDto>();

            foreach (var userId in userIds)
            {
                notifications.Add(await NotifyUserAsync(userId, title, message, type, entityType, entityId));
            }

            return notifications;
        }

        public async Task<List<NotificationDto>> NotifyRecruitmentTeamAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null)
        {
            var userIds = await GetRecruitmentTeamUserIdsAsync();
            var notifications = new List<NotificationDto>();

            foreach (var userId in userIds)
            {
                notifications.Add(await NotifyUserAsync(userId, title, message, type, entityType, entityId));
            }

            return notifications;
        }

        private async Task<List<Guid>> GetAdminUserIdsAsync()
        {
            var adminPermission = Permissions.AdminAccess.ToString();
            var superAdminPermission = Permissions.SuperAdminAccess.ToString();
            var manageDepartmentStructure = Permissions.ManageDepartmentStructure.ToString();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted &&
                    (
                        u.UserPermissions.Any(up =>
                            up.Permission.Name == adminPermission ||
                            up.Permission.Name == superAdminPermission ||
                            up.Permission.Name == manageDepartmentStructure) ||
                        u.UserRoles.Any(ur =>
                            ur.Role != null &&
                            (
                                ur.Role.Name == "Admin" ||
                                ur.Role.Name == "SuperAdmin" ||
                                ur.Role.Name == "Super Admin" ||
                                ur.Role.RolePermissions.Any(rp =>
                                    rp.Permission.Name == adminPermission ||
                                    rp.Permission.Name == superAdminPermission ||
                                    rp.Permission.Name == manageDepartmentStructure)
                            ))
                    ))
                .Select(u => u.Id)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<Guid>> GetRecruitmentTeamUserIdsAsync()
        {
            var manageRecruitmentPermission = Permissions.ManageRecruitment.ToString();
            var adminPermission = Permissions.AdminAccess.ToString();
            var superAdminPermission = Permissions.SuperAdminAccess.ToString();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted &&
                    (
                        u.UserPermissions.Any(up =>
                            up.Permission.Name == manageRecruitmentPermission ||
                            up.Permission.Name == adminPermission ||
                            up.Permission.Name == superAdminPermission) ||
                        u.UserRoles.Any(ur =>
                            ur.Role != null &&
                            (
                                ur.Role.Name == "Admin" ||
                                ur.Role.Name == "SuperAdmin" ||
                                ur.Role.Name == "Super Admin" ||
                                ur.Role.Name == "HR" ||
                                ur.Role.RolePermissions.Any(rp =>
                                    rp.Permission.Name == manageRecruitmentPermission ||
                                    rp.Permission.Name == adminPermission ||
                                    rp.Permission.Name == superAdminPermission)
                            ))
                    ))
                .Select(u => u.Id)
                .Distinct()
                .ToListAsync();
        }

        private static System.Linq.Expressions.Expression<Func<Notification, NotificationDto>> MapToDto => notification => new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
