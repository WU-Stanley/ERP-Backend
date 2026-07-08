
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDetails?> GetEmployeeProfileAsync(Guid employeeId);
        Task<EmployeeDetails?> GetEmployeeByUserIdAsync(Guid userId);
        Task<IEnumerable<EmployeeDirectoryDto>> GetEmployeeDirectoryAsync();
        Task<IEnumerable<EmployeeDirectoryDto>> GetEmployeeDirectoryByDepartmentAsync(Guid departmentId);
        Task<EmployeeDetails?> UpdateOwnProfileAsync(Guid userId, EmployeeSelfServiceUpdateDto update);
        Task<EmployeeProfileUpdateRequestDto?> SubmitOwnProfileUpdateRequestAsync(Guid userId, EmployeeSelfServiceUpdateDto update);
        Task<IEnumerable<EmployeeProfileUpdateRequestDto>> GetOwnProfileUpdateRequestsAsync(Guid userId);
        Task<IEnumerable<EmployeeProfileUpdateRequestDto>> GetProfileUpdateRequestsAsync(string? status);
        Task<EmployeeProfileUpdateRequestDto?> ReviewProfileUpdateRequestAsync(Guid requestId, Guid reviewerUserId, ProfileUpdateDecisionDto decision);
        Task<EmployeeDetails> CreateEmployeeAsync(CreateUserDto employee);
        Task<BulkStaffUploadResultDto> BulkCreateEmployeesAsync(IFormFile file);
        Task<EmployeeDetails> UpdateEmployeeAsync(EmployeeDetails employee);
        Task<List<JobCategory>> GetJobCategoriesAsync();
        Task<HrSummaryReportDto> GetHrSummaryReportAsync();
    }

}
