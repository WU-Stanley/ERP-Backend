using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly WUIAMDbContext _context;

    public LeaveBalanceRepository(WUIAMDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance?> GetByUserAndTypeAsync(Guid userId, Guid leaveTypeId)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.UserId == userId && lb.LeaveTypeId == leaveTypeId);
    }

    public async Task<List<LeaveBalance>> GetByUserAsync(Guid userId)
    {
        return await _context.LeaveBalances
            .Where(lb => lb.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<LeaveBalance>> GetAllAsync()
    {
        return await _context.LeaveBalances.ToListAsync();
    }

    public async Task AddAsync(LeaveBalance balance)
    {
        await _context.LeaveBalances.AddAsync(balance);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveBalance balance)
    {
        _context.LeaveBalances.Update(balance);
        await _context.SaveChangesAsync();
    }

    public async Task SyncBalanceAsync(Guid userId, Guid leaveTypeId, int usedDays)
    {
        var balance = await GetByUserAndTypeAsync(userId, leaveTypeId);
        if (balance != null)
        {
            balance.UsedDays = usedDays;
            balance.RemainingDays = balance.TotalDays - usedDays;
            _context.LeaveBalances.Update(balance);
  
            await _context.SaveChangesAsync();
        }
    }



    public async Task<bool> ExistsAsync(Guid userId, Guid leaveTypeId)
    {
        return await _context.LeaveBalances
            .AnyAsync(lb => lb.UserId == userId && lb.LeaveTypeId == leaveTypeId);
    }
}