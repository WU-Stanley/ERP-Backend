using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IApprovalFlowRepository
    {
        Task<ApprovalFlow> AddAsync(ApprovalFlow approvalFlow);
        Task<ApprovalFlow> UpdateAsync(ApprovalFlow approvalFlow);
        Task DeleteAsync(string id);
        Task<ApprovalFlow?> GetByIdAsync(Guid id);
        Task<IEnumerable<ApprovalFlow?>> GetAllApprovalFlow();
    }
}
