using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WUIAM.Attributes;
using WUIAM.Enums;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("employees")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<IActionResult> ExportEmployees()
        {
            var csvBytes = await _exportService.ExportEmployeesCsvAsync();
            return File(csvBytes, "text/csv", "employees_export.csv");
        }

        [HttpGet("departments")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<IActionResult> ExportDepartments()
        {
            var csvBytes = await _exportService.ExportDepartmentsCsvAsync();
            return File(csvBytes, "text/csv", "departments_export.csv");
        }

        [HttpGet("leave-requests")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<IActionResult> ExportLeaveRequests()
        {
            var csvBytes = await _exportService.ExportLeaveRequestsCsvAsync();
            return File(csvBytes, "text/csv", "leave_requests_export.csv");
        }
    }
}
