using System;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly WUIAMDbContext _context;

        public LeaveRequestRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(Guid userId)
        {
            var requests =await _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .Include(lu =>lu.User)
                .Include(lr=> lr.LeaveType)
                .ThenInclude(a => a.ApprovalFlow)
                .ThenInclude(b =>b.Steps)
                .ToListAsync();
            foreach (var lt in requests)
            {
                if (lt.LeaveType.ApprovalFlow?.Steps != null)
                {
                    lt.LeaveType.ApprovalFlow.Steps = lt.LeaveType.ApprovalFlow.Steps
                        .OrderBy(s => s.StepOrder)
                        .ToList();
                }
            }
            return requests;
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests
                 .Include(lr => lr.LeaveType)
                .Include(lu => lu.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lu => lu.User).ToListAsync();
        }

        public async Task<LeaveRequest?> AddAsync(LeaveRequest request)
        {
         var l=   await _context.LeaveRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(l.Entity.Id);
        }


        public async Task<LeaveRequest> UpdateAsync(LeaveRequest request)
        {
         var l=   _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(l.Entity.Id);
        }
    }
}
