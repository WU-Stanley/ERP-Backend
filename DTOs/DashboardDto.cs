namespace WUIAM.DTOs
{
    public class DashboardStaffSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public class DashboardLeaveRequestSummaryDto
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
    }

    public class AdminDashboardSummaryDto
    {
        public int StaffCount { get; set; }
        public int RolesCount { get; set; }
        public int DepartmentsCount { get; set; }
        public int UserTypesCount { get; set; }
        public int EmploymentTypesCount { get; set; }
        public IEnumerable<DashboardStaffSummaryDto> RecentlyCreatedStaff { get; set; } = [];
    }

    public class HrDashboardSummaryDto
    {
        public int ActiveEmployees { get; set; }
        public int Departments { get; set; }
        public int EmploymentTypes { get; set; }
        public int LeaveRequestsThisMonth { get; set; }
        public int PendingApprovals { get; set; }
        public IEnumerable<DashboardLeaveRequestSummaryDto> RecentLeaveRequests { get; set; } = [];
    }

    public class LeaveDashboardSummaryDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int TotalRequests { get; set; }
        public int ApprovalQueueCount { get; set; }
        public IEnumerable<DashboardLeaveRequestSummaryDto> RecentLeaveRequests { get; set; } = [];
    }
}
