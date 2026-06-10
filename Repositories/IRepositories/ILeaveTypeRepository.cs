using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface ILeaveTypeRepository
    {
        Task<List<LeaveType>> GetAllAsync();
        Task<LeaveType?> GetByIdAsync(Guid id);

        Task AddAsync(LeaveType leaveType);

        Task UpdateAsync(LeaveType leaveType);

        Task DeleteAsync(Guid id);
        Task<LeaveTypeVisibility> getLeaveTypeVisibiltyById(Guid id);
        Task<List<LeaveType>> GetVisibleLeaveTypesForUser(Guid user);

        Task<bool> MatchesVisibility(User user, LeaveType leaveType);
    }
}
