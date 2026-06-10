using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IApprovalFlowService
    {
        Task<ApprovalFlow> CreateApprovalFlow(CreateApprovalFlowDto dto);
        Task<ApprovalFlow> UpdateApprovalFlow(ApprovalFlow approvalFlow);
        Task<ApprovalFlow> DeleteApprovalFlow(Guid id);
        Task<ApprovalFlow> GetApprovalFlow(Guid id);
        Task<IEnumerable<ApprovalFlow>> GetAllApprovalFlow();
    }
}