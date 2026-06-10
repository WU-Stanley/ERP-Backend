using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface ILeaveRequestApprovalRepository
    {
        Task<LeaveRequestApproval?> GetByIdAsync(Guid id);
        Task<List<LeaveRequestApproval>> GetByLeaveRequestIdAsync(Guid leaveRequestId);
        Task AddAsync(LeaveRequestApproval approval);

        Task UpdateAsync(LeaveRequestApproval approval);
        Task<LeaveRequestApproval?> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder);
        Task<IEnumerable<LeaveRequestApproval?>> GetByApprovalFlowIdAndApprovalPersonId(Guid? approvalFlowId, Guid? approvalPersonId);

        Task<IEnumerable<LeaveRequestApproval>> GetByApproverPersonIdAsync(Guid approverPersonId);
        Task<IEnumerable<LeaveRequestApproval>> GetByApproverDelegationPersonIdAsync(Guid value);
        Task<IEnumerable<LeaveRequestApproval>> GetAllRequestApprovals();
    }
}
