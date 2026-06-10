
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface ILeaveBalanceRepository
    {
        Task<LeaveBalance?> GetByUserAndTypeAsync(Guid userId, Guid leaveTypeId);
        Task<List<LeaveBalance>> GetByUserAsync(Guid userId);
        Task<List<LeaveBalance>> GetAllAsync();
        Task AddAsync(LeaveBalance balance);
        Task UpdateAsync(LeaveBalance balance);
        Task SyncBalanceAsync(Guid userId, Guid leaveTypeId, int usedDays);
        Task<bool> ExistsAsync(Guid userId, Guid leaveTypeId);
    }
}
