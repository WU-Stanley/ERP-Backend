
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department?> UpdateDepartmentAsync(Guid id, Department department);
        Task<Department?> DeleteDepartmentAsync(Guid id);
        Task<IEnumerable<Department>> GetNonAcademicDepartmentsAsync();
        Task<IEnumerable<Department>> GetAcademicDepartmentsAsync();
    }

}