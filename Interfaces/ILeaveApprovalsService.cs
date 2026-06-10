using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface ILeaveApprovalService
    {
        Task<ApiResponse<IEnumerable<LeaveRequestApproval>>> GetLeaveRequestApprovals(Guid leaveRequestId);
        Task<ApiResponse<LeaveRequestApproval>?> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder);
        Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApprovalFlowIdAndApprovalPersonId(Guid? approvalFlowId, Guid? approvalPersonId);
        Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApproverPersonId(Guid? approverPersonId);
        Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApproverDelegationPersonId(Guid? approverDelegationPersonId);
        Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetAllRequestApprovals();
    }
}
