using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.Interfaces;

namespace WUIAM.Repositories
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly WUIAMDbContext _context;

        public ProgramRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<AcademicProgram?> GetByIdAsync(Guid programId)
        {
            return await _context.Programs
                .Include(p => p.College)
                .FirstOrDefaultAsync(p => p.ProgramId == programId);
        }

        public async Task<IEnumerable<AcademicProgram>> GetAllAsync()
        {
            return await _context.Programs
                .Include(p => p.College)
                .ToListAsync();
        }

        public async Task<IEnumerable<AcademicProgram>> GetByCollegeAsync(Guid collegeId)
        {
            return await _context.Programs
                .Where(p => p.CollegeId == collegeId)
                .ToListAsync();
        }

        public async Task AddAsync(AcademicProgram program)
        {
            await _context.Programs.AddAsync(program);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AcademicProgram program)
        {
            _context.Programs.Update(program);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid programId)
        {
            var program = await _context.Programs.FindAsync(programId);
            if (program != null)
            {
                _context.Programs.Remove(program);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<dynamic> ExistsAsync(Guid programId)
        {
            return await _context.Programs.AnyAsync(p => p.ProgramId == programId);
        }


    }
}
