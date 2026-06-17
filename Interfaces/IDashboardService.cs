using WUIAM.DTOs;

namespace WUIAM.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<AdminDashboardSummaryDto>> GetAdminSummaryAsync();
        Task<ApiResponse<HrDashboardSummaryDto>> GetHrSummaryAsync();
        Task<ApiResponse<LeaveDashboardSummaryDto>> GetLeaveSummaryAsync();
    }
}
