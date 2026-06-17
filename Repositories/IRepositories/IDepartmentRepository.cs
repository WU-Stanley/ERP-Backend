using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department> AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<DepartmentResponseDto>> GetNonAcademicDepartmentsAsync();
        Task<IEnumerable<DepartmentResponseDto>> GetAcademicDepartmentsAsync();
    }
}