using System;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;

namespace WUIAM.Jobs
{
    public class LeaveBalanceJob
    {
        private readonly WUIAMDbContext _context;

        public LeaveBalanceJob(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task GenerateLeaveBalancesForNewCycle()
        {
            var today = DateTime.UtcNow;
            var startOfCycle = new DateTime(today.Year, 1, 1);
            var endOfCycle = new DateTime(today.Year, 12, 31);

            var users = await _context.Users.ToListAsync();
            var leaveTypes = await _context.LeaveTypes.ToListAsync();

            foreach (var user in users)
            {
                foreach (var leaveType in leaveTypes)
                {
                    bool alreadyExists = await _context.LeaveBalances.AnyAsync(lb =>
                        lb.UserId == user.Id &&
                        lb.LeaveTypeId == leaveType.Id &&
                        lb.ValidFrom == startOfCycle);

                    if (!alreadyExists)
                    {
                        var newBalance = new LeaveBalance
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            LeaveTypeId = leaveType.Id,
                            TotalDays = leaveType.MaxDays,
                            UsedDays = 0,
                            RemainingDays = leaveType.MaxDays,
                            ValidFrom = startOfCycle,
                            ValidTo = endOfCycle,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _context.LeaveBalances.AddAsync(newBalance);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }

}
