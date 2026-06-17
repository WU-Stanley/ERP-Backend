using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [HasPermission(Permissions.AdminAccess, Permissions.ViewAuditLogs, Permissions.SuperAdminAccess)]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<AuditLogPaginationDto>>> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? entityType = null,
            [FromQuery] string? actionType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? userId = null)
        {
            var result = await _auditLogService.GetLogsAsync(page, pageSize, entityType, actionType, startDate, endDate, userId);
            return Ok(ApiResponse<AuditLogPaginationDto>.Success("Audit logs retrieved successfully", result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AuditLogQueryDto>>> GetLog(Guid id)
        {
            var result = await _auditLogService.GetLogByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<AuditLogQueryDto>.Failure("Audit log not found"));

            return Ok(ApiResponse<AuditLogQueryDto>.Success("Audit log retrieved successfully", result));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AuditLogQueryDto>>>> GetLogsByUser(Guid userId)
        {
            var result = await _auditLogService.GetLogsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<AuditLogQueryDto>>.Success("User audit logs retrieved successfully", result));
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<AuditLogStatsDto>>> GetStats()
        {
            var result = await _auditLogService.GetStatsAsync();
            return Ok(ApiResponse<AuditLogStatsDto>.Success("Audit log statistics retrieved successfully", result));
        }
    }
}
