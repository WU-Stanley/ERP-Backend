using System;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class ApprovalStepRepository : IApprovalStepRepository
    {
        private readonly WUIAMDbContext _context;

        
        public ApprovalStepRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApprovalStep>> GetByFlowIdAsync(Guid flowId)
        {
            return await _context.ApprovalSteps
                .Where(x => x.ApprovalFlowId == flowId)
                .OrderBy(x => x.StepOrder)
                .ToListAsync();
        }

        public Task<ApprovalStep?> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder)
        {
            return _context.ApprovalSteps
                .FirstOrDefaultAsync(x => x.ApprovalFlowId == approvalFlowId && x.StepOrder == stepOrder);
        }
    }
}
