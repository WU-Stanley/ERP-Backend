
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Controllers
{
    /// <summary>
    /// API v1 - Leave management endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IAuthRepository _userService;

        public LeaveController(ILeaveService leaveService, IAuthRepository userService)
        {
            _leaveService = leaveService;
            _userService = userService;
        }

        //POST: /api/leave/create-leave-request
        [HttpPost("create-leave-request")]
        public async Task<ActionResult<ApiResponse<LeaveRequest>>> ApplyForLeave([FromBody] LeaveRequestCreateDto leaveRequestCreateDto)
        {

            var request = await _leaveService.ApplyForLeaveAsync(leaveRequestCreateDto);
             
                return Ok(request);
            
        }
        //PUT: /api/update-leave-request/{id}
        [HttpPut("update-leave-request/{id}")]
        public async Task<ActionResult<ApiResponse<LeaveRequest>>> UpdateLeaveRequest(Guid id, [FromBody] LeaveRequestCreateDto leaveRequestCreateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var request = await _leaveService.UpdateLeaveRequestAsync(id, leaveRequestCreateDto);
             
                return Ok(request); 
        }

      
        /// <summary>
        /// Get all leave requests with pagination.
        /// </summary>
        [HasPermission([Permissions.ApproveRequests, Permissions.AdminAccess, Permissions.ManageLeaveRequests])]
        [HttpGet("all-leave-requests")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<LeaveRequest>>>> GetPendingLeaveRequests([FromQuery] PaginationParams pagination)
        {
            var result = await _leaveService.GetAllLeaveRequestsAsync();
            var totalCount = result.Count();
            var paged = result
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var response = new PaginatedResponse<LeaveRequest>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(ApiResponse<PaginatedResponse<LeaveRequest>>.Success(
                result.Any() ? "Leave requests found" : "No leave request found",
                response));
        }

        /// <summary>
        /// Get leave requests for a specific user with pagination.
        /// </summary>
        [HttpGet("user-requests/{userId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<LeaveRequest>>>> GetUserLeaveRequests(Guid userId, [FromQuery] PaginationParams pagination)
        {
            var result = await _leaveService.GetLeaveRequestsByUserAsync(userId);
            var totalCount = result.Count();
            var paged = result
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var response = new PaginatedResponse<LeaveRequest>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(ApiResponse<PaginatedResponse<LeaveRequest>>.Success(
                result.Any() ? "Leave requests found" : "No leave request found",
                response));
        }
             
    }

}
