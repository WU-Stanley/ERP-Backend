using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        private string? GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckIn([FromBody] CheckInRequestDto request)
        {
            request.IpAddress = GetClientIpAddress();
            var response = await _attendanceService.CheckInAsync(request);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("check-out")]
        public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckOut([FromBody] CheckOutRequestDto request)
        {
            request.IpAddress = GetClientIpAddress();
            var response = await _attendanceService.CheckOutAsync(request);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceRecordDto>>>> GetEmployeeAttendances(Guid employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var response = await _attendanceService.GetEmployeeAttendancesAsync(employeeId, startDate, endDate);
            return Ok(response);
        }

        [HttpGet]
        [HasPermission(Permissions.ViewAttendanceRecords, Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceRecordDto>>>> GetAllAttendances([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var response = await _attendanceService.GetAllAttendancesAsync(startDate, endDate);
            return Ok(response);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<ApiResponse<AttendanceSettingsDto>>> GetSettings()
        {
            var response = await _attendanceService.GetSettingsAsync();
            return Ok(response);
        }

        [HttpPost("settings")]
        [HasPermission(Permissions.ConfigureSystemSettings, Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<AttendanceSettingsDto>>> SaveSettings([FromBody] AttendanceSettingsDto request)
        {
            var response = await _attendanceService.SaveSettingsAsync(request);
            return Ok(response);
        }
    }
}
