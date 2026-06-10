using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
namespace WUIAM.Services
{


    public class EmploymentService : IEmploymentService
    {
        private readonly IEmploymentRepository _employmentRepo;
        private readonly IAuthService _authService;

        public EmploymentService(IEmploymentRepository employmentRepo, IAuthService authService)
        {
            _employmentRepo = employmentRepo;
            _authService = authService;
        }

        public async Task<EmploymentDetails?> GetEmploymentAsync(Guid employmentId)
            => await _employmentRepo.GetByIdAsync(employmentId);

        public async Task<IEnumerable<EmploymentDetails>> GetEmploymentHistoryAsync(Guid employeeId)
            => await _employmentRepo.GetByEmployeeIdAsync(employeeId);

        public async Task<EmploymentDetails?> GetCurrentEmploymentAsync(Guid employeeId)
            => await _employmentRepo.GetCurrentByEmployeeIdAsync(employeeId);

        public async Task<EmploymentDetails> AssignEmploymentAsync(Guid employeeId, EmploymentDetails employment)
        {
            var current = await _employmentRepo.GetCurrentByEmployeeIdAsync(employeeId);
            if (current != null)
            {
                current.IsActive = false;
                current.EndDate = DateTime.UtcNow;
                await _employmentRepo.UpdateAsync(current);
            }

            employment.EmployeeId = employeeId;
            employment.StartDate = DateTime.UtcNow;
            employment.IsActive = true;

            await _employmentRepo.AddAsync(employment);
            return employment;
        }

        public async Task EndEmploymentAsync(Guid employmentId, DateTime endDate)
        {
            var employment = await _employmentRepo.GetByIdAsync(employmentId);
            if (employment == null) return;

            employment.IsActive = false;
            employment.EndDate = endDate;

            await _employmentRepo.UpdateAsync(employment);
        }
    }
}