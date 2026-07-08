using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Services; // Adjust namespace as needed
using WUIAM.Models;
using WUIAM.Interfaces;
using WUIAM.DTOs;   // Adjust namespace as needed
using WUIAM.Enums;

namespace WUIAM.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;

        public DepartmentController(IDepartmentService departmentService, IEmployeeService employeeService)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
        }

        // GET: api/Department/reports/summary
        [HttpGet("reports/summary")]
        [HasPermission(Permissions.AdminAccess, Permissions.ViewHRReports, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentSummaryDto>>>> GetDepartmentSummaryReport()
        {
            var report = await _departmentService.GetDepartmentSummaryReportAsync();
            return Ok(ApiResponse<IEnumerable<DepartmentSummaryDto>>.Success("Department Summary Report retrieved successfully", report));
        }

        // GET: api/Department
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDto>>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return ApiResponse<IEnumerable<DepartmentResponseDto>>.Success("Department list", departments);
        }

        // GET: api/Department/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Department>>> GetById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
                return ApiResponse<Department>.Failure("Department not found");

            return ApiResponse<Department>.Success("Department found", department);
        }

        // GET: api/Department/{id}/employees
        [HttpGet("{id}/employees")]
        [HasPermission(Permissions.AdminAccess, Permissions.ViewEmployeeProfiles, Permissions.ViewDepartmentEmployeeProfiles, Permissions.ManageUsers, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDirectoryDto>>>> GetDepartmentEmployees(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
                return NotFound(ApiResponse<IEnumerable<EmployeeDirectoryDto>>.Failure("Department not found"));

            var employees = await _employeeService.GetEmployeeDirectoryByDepartmentAsync(id);
            return Ok(ApiResponse<IEnumerable<EmployeeDirectoryDto>>.Success(
                employees.Any() ? "Department employees retrieved successfully" : "No employees assigned to this department",
                employees));
        }

        // POST: api/Department
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Department>>> Create([FromBody] CreateDepartmentDto createDept)
        {
            Guid? headId = null;
            if (!string.IsNullOrWhiteSpace(createDept.HeadOfDepartmentId)
                && createDept.HeadOfDepartmentId != Guid.Empty.ToString())
            {
                if (!Guid.TryParse(createDept.HeadOfDepartmentId, out var parsedHeadId)
                    || !await _departmentService.EmployeeExistsAsync(parsedHeadId))
                {
                    return BadRequest(ApiResponse<Department>.Failure("Selected head of department is not a valid employee."));
                }

                headId = parsedHeadId;
            }

            var department = new Department
            {
                Name = createDept.Name,
                Description = createDept.Description,
                HeadId = headId
            };

            var created = await _departmentService.CreateDepartmentAsync(department);
            return ApiResponse<Department>.Success("Department created successfully", created);
            // return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/Department/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Department>>> Update(Guid id, [FromBody] CreateDepartmentDto updateDept)
        {
            if (id != updateDept.Id)
                return BadRequest(ApiResponse<Department>.Failure("Department update failed! ID mismatch."));

            Guid? headId = null;
            if (!string.IsNullOrWhiteSpace(updateDept.HeadOfDepartmentId)
                && updateDept.HeadOfDepartmentId != Guid.Empty.ToString())
            {
                if (!Guid.TryParse(updateDept.HeadOfDepartmentId, out var parsedHeadId)
                    || !await _departmentService.EmployeeExistsAsync(parsedHeadId))
                {
                    return BadRequest(ApiResponse<Department>.Failure("Selected head of department is not a valid employee."));
                }
                headId = parsedHeadId;
            }

            var existingDepartment = await _departmentService.GetByIdAsync(id);
            if (existingDepartment == null)
                return NotFound(ApiResponse<Department>.Failure("Department not found!"));

            // Update only fields provided by frontend
            existingDepartment.Name = updateDept.Name;
            existingDepartment.Description = updateDept.Description;
            existingDepartment.HeadId = headId;

            var updated = await _departmentService.UpdateDepartmentAsync(id, existingDepartment);
            if (updated == null)
                return NotFound(ApiResponse<Department>.Failure("Department not found!"));

            return Ok(ApiResponse<Department>.Success($"Department with id {updated.Id} has been updated", updated));
        }

        // DELETE: api/Department/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<Department>>> Delete(Guid id)
        {
            var deleted = await _departmentService.DeleteDepartmentAsync(id);
            if (deleted == null)
                return ApiResponse<Department>.Failure("Department not found!");

            return ApiResponse<Department>.Success("Department deleted successfully!", deleted);

        }
        [HttpGet("get-academic-departments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDto>>>> GetAcademicDepartments()
        {
            var departments = await _departmentService.GetAcademicDepartmentsAsync();
            if (departments == null || !departments.Any())
            {
                return ApiResponse<IEnumerable<DepartmentResponseDto>>.Failure("No academic department found");
            }
            return ApiResponse<IEnumerable<DepartmentResponseDto>>.Success(departments.Count() + " departments found!", departments);
        }
        [HttpGet("get-nonacademic-departments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDto>>>> GetAcademiNoncDepartments()
        {
            var departments = await _departmentService.GetNonAcademicDepartmentsAsync();
            if (departments == null || !departments.Any())
            {
                return ApiResponse<IEnumerable<DepartmentResponseDto>>.Failure("No academic department found");
            }
            return ApiResponse<IEnumerable<DepartmentResponseDto>>.Success(departments.Count() + " departments found!", departments);
        }
    }
}
