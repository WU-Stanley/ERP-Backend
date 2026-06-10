using System.Linq;
using System;
using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace WUIAM.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly WUIAMDbContext _context;

        public LeaveRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id);
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

        //public async Task<LeaveRequestApproval> ApproveOrRejectStepAsync(User user, Guid approvalId, ApprovalDecisionDto dto)
        //{
        //    var approval = await _context.LeaveRequestApprovals.FirstOrDefaultAsync(a => a.Id == approvalId && a.ApproverPersonId == user.Id);
        //    if (approval == null)
        //    {
        //        throw new UnauthorizedAccessException("Not authorized or approval step not found");
        //    }

        //    approval.Status = dto.IsApproved ? "Approved" : "Rejected";
        //    approval.Comment = dto.Comment;
        //    approval.DecisionAt = DateTime.UtcNow;
        //    _context.LeaveRequestApprovals.Update(approval);

        //    var request = await _context.LeaveRequests.FirstOrDefaultAsync(r => r.Id == approval.LeaveRequestId);
        //    if (!dto.IsApproved)
        //    {
        //        request!.Status = "Rejected";
        //    }
        //    else
        //    {
        //        var allApprovals = await _context.LeaveRequestApprovals
        //            .Where(a => a.LeaveRequestId == request!.Id)
        //            .ToListAsync();

        //        if (allApprovals.All(a => a.Status == "Approved"))
        //        {
        //            request.Status = "Approved";
        //            await CreateLeaveFromApprovedRequestAsync(request);
        //        }
        //    }

        //    _context.LeaveRequests.Update(request!);
        //    await _context.SaveChangesAsync();

        //    return approval;
        //}

        public async Task<List<LeaveRequest>> GetAllLeaveRequestsAsync()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsByUserAsync(Guid userId)
        {
            return await _context.LeaveRequests.Where(r => r.UserId == userId).ToListAsync();
        }


        public async Task<Leave> CreateLeaveFromApprovedRequestAsync(LeaveRequest request)
        {
            var publicHolidays = await _context.PublicHolidays.ToListAsync();
            int totalDays = Enumerable.Range(0, (request.EndDate - request.StartDate).Days + 1)
                .Select(offset => request.StartDate.AddDays(offset))
                .Count(day => !publicHolidays.Any(h => h.Date.Date == day.Date));

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

        public async Task<List<Leave>> GetActiveLeavesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Leaves
                .Where(l => !l.IsCancelled && l.EndDate >= today)
                .ToListAsync();
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
                var publicHolidays = await _context.PublicHolidays.ToListAsync();
                int totalDays = Enumerable.Range(0, (newEndDate - newStartDate).Days + 1)
                    .Select(offset => newStartDate.AddDays(offset))
                    .Count(day => !publicHolidays.Any(h => h.Date.Date == day.Date));

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
            // Find the specific balance record for this type and year
            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.UserId == leave.UserId
                                        && lb.LeaveTypeId == leave.LeaveTypeId
                                        && leave.StartDate >= lb.ValidFrom
                                        && lb.ValidTo >= leave.EndDate);

            if (leaveBalance != null)
            {
                // Sum only the leaves of this specific type within this specific cycle
                var usedDays = await _context.Leaves
                    .Where(l => l.UserId == leave.UserId
                             && l.LeaveTypeId == leave.LeaveTypeId
                             && !l.IsCancelled
                             && l.StartDate >= leaveBalance.ValidFrom
                             && l.EndDate <= leaveBalance.ValidTo)
                    .SumAsync(l => l.TotalDays);

                leaveBalance.UsedDays = usedDays;
                leaveBalance.RemainingDays = leaveBalance.TotalDays - usedDays;

                await _context.SaveChangesAsync();
            }
            else
            {
                // Optional: Handle case where no balance exists for the current year
                //_logger.LogWarning($"No balance found for User {leave.UserId} and Type {leave.LeaveTypeId}");
            }
        }
    }
    }
