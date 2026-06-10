using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
namespace WUIAM.Repositories
{

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly WUIAMDbContext _context;

        public EmployeeRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        // -----------------------
        // EmployeeDetails
        // -----------------------

        public async Task<EmployeeDetails?> GetByIdAsync(Guid employeeId)
        {
            return await _context.EmployeeDetails
                .Include(e => e.User)
                .Include(e => e.Employments)
                    .ThenInclude(ed => ed.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<EmployeeDetails>> GetAllAsync()
        {
            return await _context.EmployeeDetails
                .Include(e => e.Employments)
                .ToListAsync();
        }

        public async Task<EmployeeDetails> AddAsync(EmployeeDetails employee)
        {
            _context.EmployeeDetails.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<EmployeeDetails> UpdateAsync(EmployeeDetails employee)
        {
            _context.EmployeeDetails.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<EmployeeDetails> DeleteAsync(Guid employeeId)
        {
            var employee = await _context.EmployeeDetails.FindAsync(employeeId);
            if (employee != null)
            {
                _context.EmployeeDetails.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return employee!;
        }

        // -----------------------
        // EmploymentDetails
        // -----------------------

        public async Task<IEnumerable<EmploymentDetails>> GetEmploymentsAsync(Guid employeeId)
        {
            return await _context.EmploymentDetails
                .Include(ed => ed.Department)
                .Where(ed => ed.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<EmploymentDetails?> GetCurrentEmploymentAsync(Guid employeeId)
        {
            return await _context.EmploymentDetails
                .Include(ed => ed.Department)
                .FirstOrDefaultAsync(ed => ed.EmployeeId == employeeId && ed.IsActive);
        }

        // -----------------------
        // With User
        // -----------------------

        public async Task<EmployeeDetails?> GetByUserIdAsync(Guid userId)
        {
            return await _context.EmployeeDetails
                .Include(e => e.User)
                .Include(e => e.Employments)
                    .ThenInclude(ed => ed.Department)
                .FirstOrDefaultAsync(e => e.User!.Id == userId);
        }
        public async Task<List<JobCategory>> GetJobCategoriesAsync()
        {
            return await _context.JobCategories.ToListAsync();
        }
    }

}