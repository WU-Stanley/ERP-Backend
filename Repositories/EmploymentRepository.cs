using Microsoft.EntityFrameworkCore;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
namespace WUIAM.Repositories
{

    public class EmploymentRepository : IEmploymentRepository
    {
        private readonly WUIAMDbContext _context;

        public EmploymentRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<EmploymentDetails?> GetByIdAsync(Guid employmentId)
        {
            return await _context.EmploymentDetails
                .FirstOrDefaultAsync(e => e.EmploymentId == employmentId);
        }

        public async Task<IEnumerable<EmploymentDetails>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.EmploymentDetails
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<EmploymentDetails?> GetCurrentByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.EmploymentDetails
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.IsActive);
        }

        public async Task AddAsync(EmploymentDetails employment)
        {
            await _context.EmploymentDetails.AddAsync(employment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmploymentDetails employment)
        {
            _context.EmploymentDetails.Update(employment);
            await _context.SaveChangesAsync();
        }
    }
}