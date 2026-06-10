using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface ILeaveService
    {
        Task<ApiResponse<LeaveRequest>> ApplyForLeaveAsync(LeaveRequestCreateDto dto);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByUserAsync(Guid userId);
        Task<ApiResponse<LeaveRequestApproval>> ApproveOrRejectStepAsync(Guid approvalId, ApprovalDecisionDto dto);
        Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync();
        Task<LeaveType> CreateLeaveType(CreateLeaveTypeDto createLeaveTypeDto);
        Task<ApprovalFlow?> CreateApprovalFlow(CreateApprovalFlowDto createApprovalFlowDto);
        Task<ApiResponse<ApprovalDelegation>> DelegateApprovalAsync(ApprovalDelegationDto approvalDelegationDto);
        Task<ApiResponse<ApprovalDelegation>> RevokeApprovalDelegationAsync(Guid approvalDelegationId);
        Task<ApiResponse<LeaveRequest>> UpdateLeaveRequestAsync(Guid id, LeaveRequestCreateDto leaveRequestCreateDto);
        Task<ApiResponse<IEnumerable<LeaveRequestApproval>>> GetLeaveRequestApprovals(Guid leaveRequestId);
    }
}
