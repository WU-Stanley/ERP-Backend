using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin-summary")]
        public async Task<ActionResult<ApiResponse<AdminDashboardSummaryDto>>> GetAdminSummary()
        {
            return Ok(await _dashboardService.GetAdminSummaryAsync());
        }

        [HttpGet("hr-summary")]
        public async Task<ActionResult<ApiResponse<HrDashboardSummaryDto>>> GetHrSummary()
        {
            return Ok(await _dashboardService.GetHrSummaryAsync());
        }

        [HttpGet("leave-summary")]
        public async Task<ActionResult<ApiResponse<LeaveDashboardSummaryDto>>> GetLeaveSummary()
        {
            return Ok(await _dashboardService.GetLeaveSummaryAsync());
        }
    }
}
