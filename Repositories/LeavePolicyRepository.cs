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
                DependentLeaveTypeId = leavePolicy.DependentLeaveTypeId,
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

            System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] User ID: {user.Id}, LeaveTypeId: {leaveTypeId}");
            System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] UserRoles count: {user.UserRoles.Count}, Roles: {string.Join(", ", userRoleNames)}");
            System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] Employee found: {employee != null}, EmployeeCode: {employee?.EmployeeCode}");
            if (employee != null)
            {
                foreach (var emp in employee.Employments)
                {
                    System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] Employment: Active={emp.IsActive}, EmploymentTypeId={emp.EmploymentTypeId}");
                }
            }

            var policies = await _context.LeavePolicies
                .Where(p => p.LeaveTypeId == leaveTypeId)
                .Include(p => p.LeaveType)
                .Include(em => em.EmploymentType)
                .ToListAsync();

            System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] Total policies found for this leave type: {policies.Count}");
            foreach (var p in policies)
            {
                System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] Policy ID: {p.Id}, EmploymentTypeId: {p.EmploymentTypeId}, RoleName: {p.RoleName}");
            }

            // Prioritize policies that match both employment type and role
            var matchedPolicy = policies
                .Where(p =>
                    (p.EmploymentTypeId == null || (employee != null && employee.Employments.Any(a => a.EmploymentTypeId == p.EmploymentTypeId))) &&
                    (string.IsNullOrEmpty(p.RoleName) || userRoleNames.Contains(p.RoleName))
                )
                .OrderByDescending(p => p.LeaveType != null && !string.IsNullOrEmpty(p.LeaveType.Name))
                .ThenByDescending(p => !string.IsNullOrEmpty(p.RoleName))
                .FirstOrDefault();

            System.Console.WriteLine($"[DEBUG GetApplicablePolicyAsync] MatchedPolicy ID: {matchedPolicy?.Id}");
            return matchedPolicy;
        }


    }
}