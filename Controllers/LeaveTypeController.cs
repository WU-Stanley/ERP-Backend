using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IAuthRepository _userService;

        public LeaveTypeController(ILeaveTypeService leaveTypeService, IAuthRepository userService)
        {
            _leaveTypeService = leaveTypeService;
            _userService = userService;
        }

        // POST: api/leavetype/create
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<LeaveType>>> CreateLeaveType([FromBody] CreateLeaveTypeDto createLeaveTypeDto)
        {
            var result = await _leaveTypeService.CreateLeaveType(createLeaveTypeDto);
            if (result != null)
            {
                return ApiResponse<LeaveType>.Success("Leave type created", result);
            }
            return ApiResponse<LeaveType>.Failure("Failed to create leave type");
        }

        // GET: api/leavetype/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<LeaveType>>> GetLeaveTypeById(Guid id)
        {
            var result = await _leaveTypeService.GetLeaveTypeById(id);
            if (result != null)
            {
                return ApiResponse<LeaveType>.Success("Leave type found", result);
            }
            return ApiResponse<LeaveType>.Failure("Leave type not found");
        }

        // GET: api/leavetype/all
        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveType>>>> GetAllLeaveTypes()
        {
            var result = await _leaveTypeService.GetAllLeaveTypes();
            return ApiResponse<IEnumerable<LeaveType>>.Success("Leave types retrieved", result);
        }

        // PUT: api/leavetype/update/{id}
        [HttpPut("update/{id:guid}")]
        public async Task<ActionResult<ApiResponse<LeaveType>>> UpdateLeaveType(Guid id, [FromBody] LeaveType leaveType)
        {
            if (id != leaveType.Id)
            {
                return ApiResponse<LeaveType>.Failure("ID mismatch");
            }
            var result = await _leaveTypeService.UpdateLeaveType(leaveType);
            if (result != null)
            {
                return ApiResponse<LeaveType>.Success("Leave type updated", result);
            }
            return ApiResponse<LeaveType>.Failure("Failed to update leave type");
        }

        // DELETE: api/leavetype/delete/{id}
        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult<ApiResponse<LeaveType>>> DeleteLeaveType(Guid id)
        {
            var result = await _leaveTypeService.DeleteLeaveType(id);
            if (result != null)
            {
                return ApiResponse<LeaveType>.Success("Leave type deleted", result);
            }
            return ApiResponse<LeaveType>.Failure("Failed to delete leave type");
        }

        // GET: api/leavetype/available
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveType>>>> AvailableLeaveTypes()
        {
            var availableLeaveTypes =await _leaveTypeService.GetAvailableLeaveTypes();
            if(availableLeaveTypes != null)
            {
                return Ok(ApiResponse<IEnumerable<LeaveType>>.Success("available leave types found!", availableLeaveTypes));
            }
            return BadRequest(ApiResponse<IEnumerable<LeaveType>>.Failure("Error occured"));

        }

    }
}
