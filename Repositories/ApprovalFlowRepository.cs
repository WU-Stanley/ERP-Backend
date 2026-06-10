using System;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class ApprovalFlowRepository : IApprovalFlowRepository
    {
        private readonly WUIAMDbContext _context;

        public ApprovalFlowRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<ApprovalFlow> AddAsync(ApprovalFlow approvalFlow)
        {
            if (approvalFlow == null)
                throw new ArgumentNullException(nameof(approvalFlow));

            _context.ApprovalFlows.Add(approvalFlow);
            await _context.SaveChangesAsync();
            return approvalFlow;
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));

            if (!Guid.TryParse(id, out var guid))
                throw new ArgumentException("Id is not a valid GUID.", nameof(id));

            var approvalFlow = await _context.ApprovalFlows.FirstOrDefaultAsync(x => x.Id == guid);
            if (approvalFlow == null)
                throw new InvalidOperationException("ApprovalFlow not found.");

            _context.ApprovalFlows.Remove(approvalFlow);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApprovalFlow?>> GetAllApprovalFlow()
        {
            return await _context.ApprovalFlows.Include(a =>a.Steps).ToListAsync();
        }

        public async Task<ApprovalFlow?> GetByIdAsync(Guid id)
        {
            return await _context.ApprovalFlows.Include(a  =>a.Steps).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ApprovalFlow> UpdateAsync(ApprovalFlow approvalFlow)
        {
            if (approvalFlow == null)
                throw new ArgumentNullException(nameof(approvalFlow));

            var existing = await _context.ApprovalFlows.FirstOrDefaultAsync(x => x.Id == approvalFlow.Id);
            if (existing == null)
                throw new InvalidOperationException("ApprovalFlow not found.");
            existing.Steps = approvalFlow.Steps;
            existing.Name = approvalFlow.Name;

            _context.ApprovalFlows.Update(existing);
            await _context.SaveChangesAsync();
            return approvalFlow;
        }
    }

}
