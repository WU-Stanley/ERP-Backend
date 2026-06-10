
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
    [ApiController]
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

      
        //GET: /api/leave/pending-leave-requests
        [HasPermission([Permissions.ApproveRequests, Permissions.AdminAccess, Permissions.ManageLeaveRequests])]
        [HttpGet("all-leave-requests")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequest>>>> GetPendingLeaveRequests()
        {
            var result = await _leaveService.GetAllLeaveRequestsAsync();
            return Ok(ApiResponse<IEnumerable<LeaveRequest>>.Success(result.Count() <= 0 ? "Leave request found" : "No leave request found", result));
        }
        //GET: /api/leave/get-user-request
        [HttpGet("user-requests/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequest>>>> GetUserLeaveRequests(Guid userId)
        {
            var result = await _leaveService.GetLeaveRequestsByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<LeaveRequest>>.Success(result.Count() <= 0 ? "Leave request found" : "No leave request found", result));

        }
             
    }

}

