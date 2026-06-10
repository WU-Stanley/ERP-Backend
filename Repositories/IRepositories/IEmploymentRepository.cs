using WUIAM.Models;
namespace WUIAM.Repositories.IRepositories
{


    public interface IEmploymentRepository
    {
        Task<EmploymentDetails?> GetByIdAsync(Guid employmentId);
        Task<IEnumerable<EmploymentDetails>> GetByEmployeeIdAsync(Guid employeeId);
        Task<EmploymentDetails?> GetCurrentByEmployeeIdAsync(Guid employeeId);
        Task AddAsync(EmploymentDetails employment);
        Task UpdateAsync(EmploymentDetails employment);
    }
}