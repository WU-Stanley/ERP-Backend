using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string actionType, Guid? userId = null, string? entityName = null,
            Guid? entityId = null, string? description = null, string? oldValues = null,
            string? newValues = null, string? ipAddress = null, string? userAgent = null);

        Task<AuditLogPaginationDto> GetLogsAsync(int page = 1, int pageSize = 50,
            string? entityType = null, string? actionType = null,
            DateTime? startDate = null, DateTime? endDate = null,
            Guid? userId = null);

        Task<AuditLogQueryDto?> GetLogByIdAsync(Guid id);
        Task<IEnumerable<AuditLogQueryDto>> GetLogsByUserIdAsync(Guid userId, int limit = 100);
        Task<AuditLogStatsDto> GetStatsAsync();
    }
}
