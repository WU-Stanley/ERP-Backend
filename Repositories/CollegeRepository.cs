using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.Interfaces;

namespace WUIAM.Repositories.Implementations
{
    public class CollegeRepository : ICollegeRepository
    {
        private readonly WUIAMDbContext _context;

        public CollegeRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<College?> GetByIdAsync(Guid collegeId)
        {
            return await _context.Colleges
                .Include(c => c.Programs) // load academic programs under the college
                .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        }

        public async Task<IEnumerable<College>> GetAllAsync()
        {
            return await _context.Colleges
                .Include(c => c.Programs)
                .ToListAsync();
        }

        public async Task AddAsync(College college)
        {
            await _context.Colleges.AddAsync(college);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(College college)
        {
            _context.Colleges.Update(college);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid collegeId)
        {
            var college = await _context.Colleges.FindAsync(collegeId);
            if (college != null)
            {
                _context.Colleges.Remove(college);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid collegeId)
        {
            return await _context.Colleges.AnyAsync(c => c.CollegeId == collegeId);
        }
    }
}
