using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface ILeavePolicyRepository
    {
        Task<LeavePolicy?> GetByIdAsync(Guid id);
        Task<List<LeavePolicy>> GetAllAsync();
        Task<LeavePolicy> AddAsync(LeavePolicyDto leavePolicy);
        Task UpdateAsync(LeavePolicy leavePolicy);
        Task DeleteAsync(Guid id);
        Task<List<LeavePolicy>> GetPoliciesByLeaveTypeAsync(Guid leaveTypeId);
        Task<LeavePolicy?> GetApplicablePolicyAsync(User user, Guid leaveTypeId);
    }
}