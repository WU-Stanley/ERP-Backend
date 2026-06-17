
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department?> UpdateDepartmentAsync(Guid id, Department department);
        Task<Department?> DeleteDepartmentAsync(Guid id);
        Task<IEnumerable<DepartmentResponseDto>> GetNonAcademicDepartmentsAsync();
        Task<IEnumerable<DepartmentResponseDto>> GetAcademicDepartmentsAsync();
        Task<bool> EmployeeExistsAsync(Guid employeeId);
    }

}
