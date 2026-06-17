using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly WUIAMDbContext _dbContext;

        public AuditLogRepository(WUIAMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            await _dbContext.AuditLogs.AddAsync(auditLog);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.EntityName == entityType)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            return await _dbContext.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await _dbContext.AuditLogs.FindAsync(id);
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await _dbContext.AuditLogs.CountAsync(a => a.UserId == userId);
        }

        public async Task<int> GetCountByEntityTypeAsync(string entityType)
        {
            return await _dbContext.AuditLogs.CountAsync(a => a.EntityName == entityType);
        }
    }
}
