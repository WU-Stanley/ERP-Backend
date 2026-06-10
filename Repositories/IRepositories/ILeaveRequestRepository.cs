using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface ILeaveRequestRepository
    {
        Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(Guid userId);
        Task<LeaveRequest?> GetByIdAsync(Guid id);

        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest> AddAsync(LeaveRequest request);

        Task<LeaveRequest> UpdateAsync(LeaveRequest request);
    }
}
