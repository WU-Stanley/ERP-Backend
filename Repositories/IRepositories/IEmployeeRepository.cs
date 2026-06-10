using WUIAM.Models;
namespace WUIAM.Repositories.IRepositories
{

    public interface IEmployeeRepository
    {
        // Employee Details
        Task<EmployeeDetails?> GetByIdAsync(Guid employeeId);
        Task<IEnumerable<EmployeeDetails>> GetAllAsync();
        Task<EmployeeDetails> AddAsync(EmployeeDetails employee);
        Task<EmployeeDetails> UpdateAsync(EmployeeDetails employee);
        Task<EmployeeDetails> DeleteAsync(Guid employeeId);

        // Employment Details
        Task<IEnumerable<EmploymentDetails>> GetEmploymentsAsync(Guid employeeId);
        Task<EmploymentDetails?> GetCurrentEmploymentAsync(Guid employeeId);

        // With User
        Task<EmployeeDetails?> GetByUserIdAsync(Guid userId);
        Task<List<JobCategory>> GetJobCategoriesAsync();
    }
}