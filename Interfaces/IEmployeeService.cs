
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDetails?> GetEmployeeProfileAsync(Guid employeeId);
        Task<EmployeeDetails?> GetEmployeeByUserIdAsync(Guid userId);
        Task<EmployeeDetails> CreateEmployeeAsync(CreateUserDto employee);
        Task<EmployeeDetails> UpdateEmployeeAsync(EmployeeDetails employee);
        Task<List<JobCategory>> GetJobCategoriesAsync();
    }

}