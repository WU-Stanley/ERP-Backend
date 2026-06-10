using WUIAM.Models;
namespace WUIAM.Interfaces
{

    public interface IEmploymentService
    {
        Task<EmploymentDetails?> GetEmploymentAsync(Guid employmentId);
        Task<IEnumerable<EmploymentDetails>> GetEmploymentHistoryAsync(Guid employeeId);
        Task<EmploymentDetails?> GetCurrentEmploymentAsync(Guid employeeId);

        Task<EmploymentDetails> AssignEmploymentAsync(Guid employeeId, EmploymentDetails employment);
        Task EndEmploymentAsync(Guid employmentId, DateTime endDate);
    }
}