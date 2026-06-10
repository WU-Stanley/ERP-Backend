using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{

    public class LeavePolicyRepository : ILeavePolicyRepository

    {
        private readonly WUIAMDbContext _context;

        public LeavePolicyRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<LeavePolicy?> GetByIdAsync(Guid id)
        {
            return await _context.LeavePolicies.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<LeavePolicy>> GetAllAsync()
        {
            return await _context.LeavePolicies
                .Include(p => p.LeaveType)
                .Include(lp =>lp.EmploymentType)
                .ToListAsync();
        }

        public async Task<LeavePolicy> AddAsync(LeavePolicyDto leavePolicy)
        {
            //Guid.TryParse(leavePolicy.EmploymentTypeId, out Guid employmentTypeId);
            var entity = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveTypeId = leavePolicy.LeaveTypeId,
                EmploymentTypeId = leavePolicy.EmploymentTypeId,
                RoleName = leavePolicy.RoleName,
                AnnualEntitlement = leavePolicy.AnnualEntitlement,
                IsAccrualBased = leavePolicy.IsAccrualBased,
                AccrualRatePerMonth = leavePolicy.AccrualRatePerMonth,
                MaxCarryOverDays = leavePolicy.MaxCarryOverDays,
                AllowNegativeBalance = leavePolicy.AllowNegativeBalance,
                CreatedAt = leavePolicy.CreatedAt
            };

            await _context.LeavePolicies.AddAsync(entity);
            await _context.SaveChangesAsync();
      return entity;
        }

        public async Task UpdateAsync(LeavePolicy leavePolicy)
        {
            _context.LeavePolicies.Update(leavePolicy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var leavePolicy = await GetByIdAsync(id);
            if (leavePolicy != null)
            {
                _context.LeavePolicies.Remove(leavePolicy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<LeavePolicy>> GetPoliciesByLeaveTypeAsync(Guid leaveTypeId)
        {
            return await _context.LeavePolicies
                .Where(lp => lp.LeaveTypeId == leaveTypeId)
                .ToListAsync();
        }

        public async Task<LeavePolicy?> GetApplicablePolicyAsync(User user, Guid leaveTypeId)
        {
            var userRoleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var employee = await _context.EmployeeDetails.Include(e => e.Employments).FirstOrDefaultAsync(a => a.UserId == user.Id);
            

            var policies = await _context.LeavePolicies
                .Where(p => p.LeaveTypeId == leaveTypeId).Include(em =>em.EmploymentType)
                .ToListAsync();

            // Prioritize policies that match both employment type and role
            var matchedPolicy = policies
                .Where(p =>
                    (string.IsNullOrEmpty(p.EmploymentTypeId.ToString()) ||employee.Employments.Any(a=>a.EmployeeId == p.EmploymentType.Id ) )&&
                    (string.IsNullOrEmpty(p.RoleName) || userRoleNames.Contains(p.RoleName))
                )
                .OrderByDescending(p => !string.IsNullOrEmpty(p.LeaveType.Name.ToString()))
                .ThenByDescending(p => !string.IsNullOrEmpty(p.RoleName))
                .FirstOrDefault();

            return matchedPolicy;
        }


    }
}