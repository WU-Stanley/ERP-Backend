using Microsoft.EntityFrameworkCore;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IApprovalStepRepository
    {
        Task<List<ApprovalStep>> GetByFlowIdAsync(Guid flowId);
        Task<ApprovalStep?> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder);
    }
}
