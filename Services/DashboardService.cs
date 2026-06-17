using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly WUIAMDbContext _dbContext;

        public DashboardService(WUIAMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<AdminDashboardSummaryDto>> GetAdminSummaryAsync()
        {
            var staffTypeIds = await _dbContext.UserTypes
                .Where(ut => ut.Name.ToLower().Contains("staff"))
                .Select(ut => ut.Id)
                .ToListAsync();

            var staffQuery = _dbContext.Users
                .Where(u => staffTypeIds.Contains(u.UserTypeId));

            var summary = new AdminDashboardSummaryDto
            {
                StaffCount = await staffQuery.CountAsync(),
                RolesCount = await _dbContext.Roles.CountAsync(),
                DepartmentsCount = await _dbContext.Departments.CountAsync(),
                UserTypesCount = await _dbContext.UserTypes.CountAsync(),
                EmploymentTypesCount = await _dbContext.EmploymentTypes.CountAsync(),
                RecentlyCreatedStaff = await staffQuery
                    .OrderByDescending(u => u.DateCreated)
                    .Take(5)
                    .Select(u => new DashboardStaffSummaryDto
                    {
                        Id = u.Id,
                        FullName = u.FirstName + " " + u.LastName,
                        Email = u.UserEmail,
                        DateCreated = u.DateCreated
                    })
                    .ToListAsync()
            };

            return ApiResponse<AdminDashboardSummaryDto>.Success("Admin dashboard summary found", summary);
        }

        public async Task<ApiResponse<HrDashboardSummaryDto>> GetHrSummaryAsync()
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var recentLeaveRequests = await BuildRecentLeaveRequestQuery().ToListAsync();

            var summary = new HrDashboardSummaryDto
            {
                ActiveEmployees = await _dbContext.EmploymentDetails
                    .Where(e => e.IsActive && e.EmploymentStatus.ToLower() == "active")
                    .Select(e => e.EmployeeId)
                    .Distinct()
                    .CountAsync(),
                Departments = await _dbContext.Departments.CountAsync(),
                EmploymentTypes = await _dbContext.EmploymentTypes.CountAsync(),
                LeaveRequestsThisMonth = await _dbContext.LeaveRequests
                    .CountAsync(lr => lr.AppliedAt >= monthStart),
                PendingApprovals = await _dbContext.LeaveRequestApprovals
                    .CountAsync(a => a.Status.ToLower() == StatusConstants.Pending.ToLower()),
                RecentLeaveRequests = recentLeaveRequests
            };

            return ApiResponse<HrDashboardSummaryDto>.Success("HR dashboard summary found", summary);
        }

        public async Task<ApiResponse<LeaveDashboardSummaryDto>> GetLeaveSummaryAsync()
        {
            var pending = StatusConstants.Pending.ToLower();
            var approved = StatusConstants.Approved.ToLower();
            var rejected = StatusConstants.Rejected.ToLower();
            var recentLeaveRequests = await BuildRecentLeaveRequestQuery().ToListAsync();

            var summary = new LeaveDashboardSummaryDto
            {
                Pending = await _dbContext.LeaveRequests.CountAsync(lr => lr.Status.ToLower() == pending),
                Approved = await _dbContext.LeaveRequests.CountAsync(lr => lr.Status.ToLower() == approved),
                Rejected = await _dbContext.LeaveRequests.CountAsync(lr => lr.Status.ToLower() == rejected),
                TotalRequests = await _dbContext.LeaveRequests.CountAsync(),
                ApprovalQueueCount = await _dbContext.LeaveRequestApprovals.CountAsync(a => a.Status.ToLower() == pending),
                RecentLeaveRequests = recentLeaveRequests
            };

            return ApiResponse<LeaveDashboardSummaryDto>.Success("Leave dashboard summary found", summary);
        }

        private IQueryable<DashboardLeaveRequestSummaryDto> BuildRecentLeaveRequestQuery()
        {
            return _dbContext.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.LeaveType)
                .OrderByDescending(lr => lr.AppliedAt)
                .Take(6)
                .Select(lr => new DashboardLeaveRequestSummaryDto
                {
                    Id = lr.Id,
                    EmployeeName = lr.User == null ? "Unknown staff" : lr.User.FirstName + " " + lr.User.LastName,
                    LeaveTypeName = lr.LeaveType == null ? "Leave" : lr.LeaveType.Name,
                    Status = lr.Status,
                    AppliedAt = lr.AppliedAt,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    TotalDays = lr.TotalDays
                });
        }
    }
}
