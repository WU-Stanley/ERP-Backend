using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department> GetByIdAsync(Guid id);
        Task<Department> AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Department>> GetNonAcademicDepartmentsAsync();
        Task<IEnumerable<Department>> GetAcademicDepartmentsAsync();
    }
}