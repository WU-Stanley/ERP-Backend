using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly WUIAMDbContext _context;

        public StudentRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Program)
                    .ThenInclude(p => p.College)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(Guid studentId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Program)
                    .ThenInclude(p => p.College)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Program)
                    .ThenInclude(p => p.College)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Student>> GetByCollegeIdAsync(Guid collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Program)
                .Where(s => s.CollegeId == collegeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByProgramIdAsync(Guid programId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Program)
                .Where(s => s.ProgramId == programId)
                .ToListAsync();
        }
    }
}
