using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{ 
        public interface ILeaveRepository
        {
            Task<LeaveRequest?> GetByIdAsync(Guid id);
            Task<List<LeaveRequest>> GetAllAsync();
            Task AddAsync(LeaveRequest leaveRequest);
            Task UpdateAsync(LeaveRequest leaveRequest);
            Task DeleteAsync(Guid id);

            Task<LeaveRequest> ApplyForLeaveAsync(User user, LeaveRequestCreateDto dto);
            //Task<LeaveRequestApproval> ApproveOrRejectStepAsync(User user, Guid approvalId, ApprovalDecisionDto dto);
            Task<List<LeaveRequest>> GetAllLeaveRequestsAsync();
            Task<List<LeaveRequest>> GetLeaveRequestsByUserAsync(Guid userId);

            Task<Leave> CreateLeaveFromApprovedRequestAsync(LeaveRequest request);
            Task<List<Leave>> GetActiveLeavesAsync();
            Task CancelLeaveAsync(Guid leaveId);
            Task ModifyLeaveAsync(Guid leaveId, DateTime newStartDate, DateTime newEndDate);
            Task SyncLeaveBalanceAsync(Leave userId);
        }
    }
