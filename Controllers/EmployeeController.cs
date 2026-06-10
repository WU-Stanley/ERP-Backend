using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;


namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> GetEmployee(Guid id)
        {
            var employee = await _employeeService.GetEmployeeProfileAsync(id);

            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDetails>.Failure($"Employee with ID {id} not found."));
            }

            return Ok(ApiResponse<EmployeeDetails>.Success("Employee retrieved successfully", employee));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> GetEmployeeByUserId(Guid userId)
        {
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDetails>.Failure($"Employee for User {userId} not found."));
            }

            return Ok(ApiResponse<EmployeeDetails>.Success("Employee retrieved successfully", employee));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateUserDto userDto)
        {
            try
            {
                var createdEmployee = await _employeeService.CreateEmployeeAsync(userDto);

                return Ok(ApiResponse<EmployeeDetails>.Success(
                    "Employee created successfully",
                    createdEmployee
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EmployeeDetails>.Failure(
                    "Failed to create employee",
                    ex.Message
                ));
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> UpdateEmployee(Guid id, EmployeeDetails employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest(ApiResponse<EmployeeDetails>.Failure("Employee ID mismatch"));
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                return Ok(ApiResponse<EmployeeDetails>.Success("Employee updated successfully", employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EmployeeDetails>.Failure("Failed to update employee", ex.Message));
            }
        }
        [AllowAnonymous]
        [HttpGet("GetJobCategories")]
        public async Task<ActionResult<ApiResponse<List<JobCategory>>>> GetJobCategories()
        {
            try
            {
                var jobCategories = await _employeeService.GetJobCategoriesAsync();
                return Ok(ApiResponse<List<JobCategory>>.Success("Job categories retrieved successfully", jobCategories));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<JobCategory>>.Failure("Failed to retrieve job categories", ex.Message));
            }
        }
    }
}