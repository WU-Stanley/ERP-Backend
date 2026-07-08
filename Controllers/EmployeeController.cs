using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Attributes;
using WUIAM.Interfaces;
using WUIAM.Models;


namespace WUIAM.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("reports/hr-summary")]
        [HasPermission(Permissions.AdminAccess, Permissions.ViewHRReports, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<HrSummaryReportDto>>> GetHrSummaryReport()
        {
            var report = await _employeeService.GetHrSummaryReportAsync();
            return Ok(ApiResponse<HrSummaryReportDto>.Success("HR Summary Report retrieved successfully", report));
        }

        /// <summary>
        /// Get a paginated list of all employees.
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.AdminAccess, Permissions.ViewEmployeeProfiles, Permissions.ViewDepartmentEmployeeProfiles, Permissions.ManageUsers, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<EmployeeDirectoryDto>>>> GetEmployees([FromQuery] PaginationParams pagination)
        {
            var employees = await _employeeService.GetEmployeeDirectoryAsync();
            var totalCount = employees.Count();
            var paged = employees
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var response = new PaginatedResponse<EmployeeDirectoryDto>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(ApiResponse<PaginatedResponse<EmployeeDirectoryDto>>.Success(
                employees.Any() ? "Employees retrieved successfully" : "No employees found",
                response));
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> GetOwnEmployee()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<EmployeeDetails>.Failure("User identity is missing."));
            }

            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId.Value);
            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDetails>.Failure("Employee profile was not found for this user."));
            }

            return Ok(ApiResponse<EmployeeDetails>.Success("Employee profile retrieved successfully", employee));
        }

        [HttpPost("me/upload-document")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<string>>> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.Failure("No file was uploaded."));
            }

            try
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "employees");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"uploads/employees/{fileName}";
                return Ok(ApiResponse<string>.Success("File uploaded successfully", relativePath));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failure("File upload failed", ex.Message));
            }
        }

        [HttpPatch("me/self-service")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> UpdateOwnEmployee([FromBody] EmployeeSelfServiceUpdateDto update)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<EmployeeDetails>.Failure("User identity is missing."));
            }

            var employee = await _employeeService.UpdateOwnProfileAsync(userId.Value, update);
            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDetails>.Failure("Employee profile was not found for this user."));
            }

            return Ok(ApiResponse<EmployeeDetails>.Success("Self-service profile updated successfully", employee));
        }

        [HttpPost("me/profile-update-requests")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<EmployeeProfileUpdateRequestDto>>> SubmitOwnProfileUpdateRequest([FromBody] EmployeeSelfServiceUpdateDto update)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<EmployeeProfileUpdateRequestDto>.Failure("User identity is missing."));
            }

            var request = await _employeeService.SubmitOwnProfileUpdateRequestAsync(userId.Value, update);
            if (request == null)
            {
                return NotFound(ApiResponse<EmployeeProfileUpdateRequestDto>.Failure("Employee profile was not found for this user."));
            }

            return Ok(ApiResponse<EmployeeProfileUpdateRequestDto>.Success("Profile update request submitted for review.", request));
        }

        [HttpGet("me/profile-update-requests")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeProfileUpdateRequestDto>>>> GetOwnProfileUpdateRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<IEnumerable<EmployeeProfileUpdateRequestDto>>.Failure("User identity is missing."));
            }

            var requests = await _employeeService.GetOwnProfileUpdateRequestsAsync(userId.Value);
            return Ok(ApiResponse<IEnumerable<EmployeeProfileUpdateRequestDto>>.Success("Profile update requests retrieved successfully", requests));
        }

        [HttpGet("profile-update-requests")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveProfileUpdateInDepartment, Permissions.UpdateEmployeeProfiles, Permissions.ManageUsers, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeProfileUpdateRequestDto>>>> GetProfileUpdateRequests([FromQuery] string? status = null)
        {
            var requests = await _employeeService.GetProfileUpdateRequestsAsync(status);
            return Ok(ApiResponse<IEnumerable<EmployeeProfileUpdateRequestDto>>.Success("Profile update requests retrieved successfully", requests));
        }

        [HttpPost("profile-update-requests/{requestId:guid}/decision")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveProfileUpdateInDepartment, Permissions.UpdateEmployeeProfiles, Permissions.ManageUsers, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<EmployeeProfileUpdateRequestDto>>> ReviewProfileUpdateRequest(Guid requestId, [FromBody] ProfileUpdateDecisionDto decision)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<EmployeeProfileUpdateRequestDto>.Failure("User identity is missing."));
            }

            var request = await _employeeService.ReviewProfileUpdateRequestAsync(requestId, userId.Value, decision);
            if (request == null)
            {
                return BadRequest(ApiResponse<EmployeeProfileUpdateRequestDto>.Failure("Profile update request was not found or has already been processed."));
            }

            return Ok(ApiResponse<EmployeeProfileUpdateRequestDto>.Success("Profile update request processed successfully", request));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> GetEmployee(Guid id)
        {
            var employee = await _employeeService.GetEmployeeProfileAsync(id);

            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDetails>.Failure($"Employee with ID {id} not found."));
            }

            return Ok(ApiResponse<EmployeeDetails>.Success("Employee retrieved successfully", employee));
        }

        [HttpGet("user/{userId:guid}")]
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

        [HttpPost("bulk-upload")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<ApiResponse<BulkStaffUploadResultDto>>> BulkUploadEmployees(IFormFile file)
        {
            try
            {
                var result = await _employeeService.BulkCreateEmployeesAsync(file);
                return Ok(ApiResponse<BulkStaffUploadResultDto>.Success("Bulk staff upload completed", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BulkStaffUploadResultDto>.Failure("Bulk staff upload failed", ex.Message));
            }
        }


        [HttpPut("{id:guid}")]
        [HasPermission(Permissions.AdminAccess, Permissions.UpdateEmployeeProfiles, Permissions.ManageUsers, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<EmployeeDetails>>> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateDto update)
        {
            if (id != update.EmployeeId)
            {
                return BadRequest(ApiResponse<EmployeeDetails>.Failure("Employee ID mismatch"));
            }

            try
            {
                var employee = new EmployeeDetails
                {
                    EmployeeId = update.EmployeeId,
                    FirstName = update.FirstName,
                    LastName = update.LastName,
                    MiddleName = update.MiddleName,
                    DateOfBirth = update.DateOfBirth,
                    Gender = update.Gender,
                    MaritalStatus = update.MaritalStatus,
                    Nationality = update.Nationality,
                    Address = update.Address,
                    PhoneNumber = update.PhoneNumber,
                    Email = update.Email,
                    EmergencyContactName = update.EmergencyContactName,
                    EmergencyContactPhone = update.EmergencyContactPhone,
                    Relationship = update.Relationship,
                    BankName = update.BankName,
                    BankAccountNumber = update.BankAccountNumber,
                    ProfilePicture = update.ProfilePicture
                };

                var updated = await _employeeService.UpdateEmployeeAsync(employee);
                return Ok(ApiResponse<EmployeeDetails>.Success("Employee updated successfully", updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EmployeeDetails>.Failure("Failed to update employee", ex.Message));
            }
        }
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

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : null;
        }
    }
}
