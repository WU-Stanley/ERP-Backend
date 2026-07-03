using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WUIAMDbContext _dbContext;

        public AuditLogService(IAuditLogRepository auditLogRepo, IHttpContextAccessor httpContextAccessor, WUIAMDbContext dbContext)
        {
            _auditLogRepo = auditLogRepo;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task LogAsync(string actionType, Guid? userId = null, string? entityName = null,
            Guid? entityId = null, string? description = null, string? oldValues = null,
            string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var validUserId = await ResolveExistingUserIdAsync(userId);
            
            Guid? impersonatorId = null;
            var impersonatorClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ImpersonatorId");
            if (impersonatorClaim != null && Guid.TryParse(impersonatorClaim.Value, out var parsedImpersonatorId))
            {
                impersonatorId = parsedImpersonatorId;
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActionType = actionType,
                UserId = validUserId,
                ImpersonatorId = impersonatorId,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = userAgent ?? _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _auditLogRepo.AddAsync(auditLog);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to write audit log: {ex}");
            }
        }

        private async Task<Guid?> ResolveExistingUserIdAsync(Guid? userId)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Users.AnyAsync(user => user.Id == userId.Value)
                ? userId
                : null;
        }

        public async Task<AuditLogPaginationDto> GetLogsAsync(int page = 1, int pageSize = 50,
            string? entityType = null, string? actionType = null,
            DateTime? startDate = null, DateTime? endDate = null,
            Guid? userId = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            var query = _dbContext.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityName == entityType);

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(a => a.ActionType == actionType);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new AuditLogPaginationDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Logs = logs.Select(a => new AuditLogQueryDto
                {
                    Id = a.Id,
                    ActionType = a.ActionType,
                    UserId = a.UserId,
                    UserName = a.User?.FullName,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Description = a.Description,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };

            return result;
        }

        public async Task<AuditLogQueryDto?> GetLogByIdAsync(Guid id)
        {
            var log = await _dbContext.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (log == null) return null;

            return new AuditLogQueryDto
            {
                Id = log.Id,
                ActionType = log.ActionType,
                UserId = log.UserId,
                UserName = log.User?.FullName,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Description = log.Description,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            };
        }

        public async Task<IEnumerable<AuditLogQueryDto>> GetLogsByUserIdAsync(Guid userId, int limit = 100)
        {
            var logs = await _dbContext.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return logs.Select(a => new AuditLogQueryDto
            {
                Id = a.Id,
                ActionType = a.ActionType,
                UserId = a.UserId,
                UserName = a.User?.FullName,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Description = a.Description,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.CreatedAt
            });
        }

        public async Task<AuditLogStatsDto> GetStatsAsync()
        {
            var totalLogs = await _dbContext.AuditLogs.CountAsync();
            var loginEvents = await _dbContext.AuditLogs.CountAsync(a => a.ActionType == "Login");
            var logoutEvents = await _dbContext.AuditLogs.CountAsync(a => a.ActionType == "Logout");
            var createEvents = await _dbContext.AuditLogs.CountAsync(a => a.ActionType == "Create");
            var updateEvents = await _dbContext.AuditLogs.CountAsync(a => a.ActionType == "Update");
            var deleteEvents = await _dbContext.AuditLogs.CountAsync(a => a.ActionType == "Delete");
            var topEntities = await _dbContext.AuditLogs
                .Where(a => a.EntityName != null && a.EntityName != string.Empty)
                .GroupBy(a => a.EntityName)
                .Select(g => new AuditLogEntityStatsDto
                {
                    EntityName = g.Key ?? string.Empty,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return new AuditLogStatsDto
            {
                TotalLogs = totalLogs,
                LoginEvents = loginEvents,
                LogoutEvents = logoutEvents,
                CreateEvents = createEvents,
                UpdateEvents = updateEvents,
                DeleteEvents = deleteEvents,
                TopEntities = topEntities
            };
        }
    }
}
