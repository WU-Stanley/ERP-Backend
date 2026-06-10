using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Services; // Adjust namespace as needed
using WUIAM.Models;
using WUIAM.Interfaces;
using WUIAM.DTOs;   // Adjust namespace as needed

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/Department
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Department>>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return ApiResponse<IEnumerable<Department>>.Success("Department list", departments);
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

        // POST: api/Department
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Department>>> Create([FromBody] CreateDepartmentDto createDept)
        {
            var department = new Department
            {
                Name = createDept.Name,
                Description = createDept.Description,
                HeadId = createDept.HeadOfDepartmentId
            };

            var created = await _departmentService.CreateDepartmentAsync(department);
            return ApiResponse<Department>.Success("Department created successfully", created);
            // return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/Department/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Department>>> Update(Guid id, [FromBody] Department department)
        {
            if (id != department.Id)
                return ApiResponse<Department>.Failure("Department update failed!");

            var updated = await _departmentService.UpdateDepartmentAsync(id, department);
            if (updated == null)
                return ApiResponse<Department>.Failure("Department not found!");

            return ApiResponse<Department>.Success("Department with id {updated.Id}  has being updated", updated);
        }

        // DELETE: api/Department/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<Department>>> Delete(Guid id)
        {
            var deleted = await _departmentService.DeleteDepartmentAsync(id);
            if (deleted == null)
                return ApiResponse<Department>.Failure("Department not found!");

            return ApiResponse<Department>.Failure("Department deleted successfully!");

        }
        [HttpGet("get-academic-departments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Department>>>> GetAcademicDepartments()
        {
            var departments = await _departmentService.GetAcademicDepartmentsAsync();
            if (departments == null || !departments.Any())
            {
                return ApiResponse<IEnumerable<Department>>.Failure("No academic department found");
            }
            return ApiResponse<IEnumerable<Department>>.Success(departments.Count() + " departments found!", departments);
        }
        [HttpGet("get-nonacademic-departments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Department>>>> GetAcademiNoncDepartments()
        {
            var departments = await _departmentService.GetNonAcademicDepartmentsAsync();
            if (departments == null || !departments.Any())
            {
                return ApiResponse<IEnumerable<Department>>.Failure("No academic department found");
            }
            return ApiResponse<IEnumerable<Department>>.Success(departments.Count() + " departments found!", departments);
        }
    }
}