using System.Linq;
using System;
using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace WUIAM.Repositories
{
    /// <summary>
    /// Optimized LeaveRepository with compiled queries and efficient balance calculations.
    /// </summary>
    public class LeaveRepository : ILeaveRepository
    {
        private readonly WUIAMDbContext _context;

        // Compiled query: get leave request by ID
        private static readonly Func<WUIAMDbContext, Guid, Task<LeaveRequest?>> _getById =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid id) =>
                db.LeaveRequests.FirstOrDefault(lr => lr.Id == id));

        // Compiled query: get leave requests by user
        private static readonly Func<WUIAMDbContext, Guid, Task<List<LeaveRequest>>> _getByUser =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId) =>
                db.LeaveRequests
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.AppliedAt)
                    .ToList());

        // Compiled query: get active leaves
        private static readonly Func<WUIAMDbContext, DateTime, Task<List<Leave>>> _getActiveLeaves =
            EF.CompileAsyncQuery((WUIAMDbContext db, DateTime today) =>
                db.Leaves
                    .Where(l => !l.IsCancelled && l.EndDate >= today)
                    .ToList());

        // Compiled query: count public holidays in date range
        private static readonly Func<WUIAMDbContext, DateTime, DateTime, Task<int>> _countHolidaysInRange =
            EF.CompileAsyncQuery((WUIAMDbContext db, DateTime start, DateTime end) =>
                db.PublicHolidays
                    .Count(h => h.Date >= start && h.Date <= end));

        // Compiled query: sum used leave days for balance sync
        private static readonly Func<WUIAMDbContext, Guid, Guid, DateTime, DateTime, Task<int>> _sumUsedDays =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId, Guid leaveTypeId, DateTime validFrom, DateTime validTo) =>
                db.Leaves
                    .Where(l => l.UserId == userId
                             && l.LeaveTypeId == leaveTypeId
                             && !l.IsCancelled
                             && l.StartDate >= validFrom
                             && l.EndDate <= validTo)
                    .Sum(l => l.TotalDays));

        // Compiled query: get leave balance for a specific user/type/year
        private static readonly Func<WUIAMDbContext, Guid, Guid, DateTime, DateTime, Task<LeaveBalance?>> _getBalance =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId, Guid leaveTypeId, DateTime validFrom, DateTime validTo) =>
                db.LeaveBalances
                    .FirstOrDefault(lb => lb.UserId == userId
                                         && lb.LeaveTypeId == leaveTypeId
                                         && validFrom >= lb.ValidFrom
                                         && validTo <= lb.ValidTo));

        public LeaveRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return _getById(_context, id);
        }

        public async Task<List<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        public async Task AddAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var leaveRequest = await GetByIdAsync(id);
            if (leaveRequest != null)
            {
                _context.LeaveRequests.Remove(leaveRequest);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LeaveRequest> ApplyForLeaveAsync(User user, LeaveRequestCreateDto dto)
        {
            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();
            return leaveRequest;
        }

        public async Task<List<LeaveRequest>> GetAllLeaveRequestsAsync()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        public Task<List<LeaveRequest>> GetLeaveRequestsByUserAsync(Guid userId)
        {
            return _getByUser(_context, userId);
        }

        public async Task<Leave> CreateLeaveFromApprovedRequestAsync(LeaveRequest request)
        {
            // Optimized: count holidays in range instead of iterating all dates
            var totalDays = (request.EndDate - request.StartDate).Days + 1
                - await _countHolidaysInRange(_context, request.StartDate, request.EndDate);

            var leave = new Leave
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = totalDays,
                Notes = request.Reason,
                CreatedAt = DateTime.UtcNow,
                IsCancelled = false,
                LeaveRequestId = request.Id
            };

            await _context.Leaves.AddAsync(leave);
            await _context.SaveChangesAsync();

            await SyncLeaveBalanceAsync(leave);
            return leave;
        }

        public Task<List<Leave>> GetActiveLeavesAsync()
        {
            return _getActiveLeaves(_context, DateTime.UtcNow.Date);
        }

        public async Task CancelLeaveAsync(Guid leaveId)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == leaveId);
            if (leave != null && !leave.IsCancelled)
            {
                leave.IsCancelled = true;
                _context.Leaves.Update(leave);
                await _context.SaveChangesAsync();

                await SyncLeaveBalanceAsync(leave);
            }
        }

        public async Task ModifyLeaveAsync(Guid leaveId, DateTime newStartDate, DateTime newEndDate)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == leaveId);
            if (leave != null && !leave.IsCancelled)
            {
                // Optimized: count holidays in range instead of iterating all dates
                int totalDays = (newEndDate - newStartDate).Days + 1
                    - await _countHolidaysInRange(_context, newStartDate, newEndDate);

                leave.StartDate = newStartDate;
                leave.EndDate = newEndDate;
                leave.TotalDays = totalDays;
                _context.Leaves.Update(leave);
                await _context.SaveChangesAsync();

                await SyncLeaveBalanceAsync(leave);
            }
        }

        public async Task SyncLeaveBalanceAsync(Leave leave)
        {
            // Optimized: use compiled query instead of re-querying DbContext
            var leaveBalance = await _getBalance(_context, leave.UserId, leave.LeaveTypeId, leave.StartDate, leave.EndDate);

            if (leaveBalance != null)
            {
                // Optimized: use compiled query for sum calculation
                var usedDays = await _sumUsedDays(_context, leave.UserId, leave.LeaveTypeId, leaveBalance.ValidFrom, leaveBalance.ValidTo);

                leaveBalance.UsedDays = usedDays;
                leaveBalance.RemainingDays = leaveBalance.TotalDays - usedDays;

                await _context.SaveChangesAsync();
            }
        }
    }
}
