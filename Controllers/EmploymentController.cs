using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;


namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmploymentDetailsController : ControllerBase
    {
        private readonly IEmploymentService _employmentService;

        public EmploymentDetailsController(IEmploymentService employmentService)
        {
            _employmentService = employmentService;
        }

        [HttpGet("{employmentId:guid}")]
        public async Task<ActionResult<ApiResponse<EmploymentDetails>>> GetEmployment(Guid employmentId)
        {
            // Get the employee that has this employment record
            var employees = await _employmentService.GetEmploymentAsync(Guid.Empty);
            // NOTE: adjust service to fetch directly by employmentId if needed

            // In a proper service: await _employmentService.GetEmploymentByIdAsync(employmentId);

            return NotFound(ApiResponse<EmploymentDetails>.Failure($"Employment with ID {employmentId} not found."));
        }

        [HttpGet("employee/{employeeId:guid}/current")]
        public async Task<ActionResult<ApiResponse<EmploymentDetails>>> GetCurrentEmployment(Guid employeeId)
        {
            var employment = await _employmentService.GetCurrentEmploymentAsync(employeeId);

            if (employment == null)
                return NotFound(ApiResponse<EmploymentDetails>.Failure($"No active employment found for employee {employeeId}"));

            return Ok(ApiResponse<EmploymentDetails>.Success("Current employment retrieved successfully", employment));
        }

        [HttpGet("employee/{employeeId:guid}/history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmploymentDetails>>>> GetEmploymentHistory(Guid employeeId)
        {
            var history = await _employmentService.GetEmploymentHistoryAsync(employeeId);

            return Ok(ApiResponse<IEnumerable<EmploymentDetails>>.Success("Employment history retrieved successfully", history.ToList()));
        }

        [HttpPost("employee/{employeeId:guid}")]
        public async Task<ActionResult<ApiResponse<EmploymentDetails>>> AssignEmployment(Guid employeeId, EmploymentDetails employment)
        {
            try
            {
                await _employmentService.AssignEmploymentAsync(employeeId, employment);

                return Ok(ApiResponse<EmploymentDetails>.Success("Employment assigned successfully", employment));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EmploymentDetails>.Failure("Failed to assign employment", ex.Message));
            }
        }

        [HttpPut("{employmentId:guid}/end")]
        public async Task<ActionResult<ApiResponse<object>>> EndEmployment(Guid employmentId)
        {
            try
            {
                await _employmentService.EndEmploymentAsync(employmentId, DateTime.UtcNow);
                return Ok(ApiResponse<object>.Success("Employment ended successfully", new { EmploymentId = employmentId }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure("Failed to end employment", ex.Message));
            }
        }
    }
}