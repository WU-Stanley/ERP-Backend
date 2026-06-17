using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<AuditLog?> GetByIdAsync(Guid id);
        Task<int> GetCountByUserIdAsync(Guid userId);
        Task<int> GetCountByEntityTypeAsync(string entityType);
    }
}
