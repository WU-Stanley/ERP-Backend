
using WUIAM.Models;

namespace WUIAM.Repositories.Interfaces
{
    public interface IProgramRepository
    {
        Task<AcademicProgram?> GetByIdAsync(Guid programId);
        Task<IEnumerable<AcademicProgram>> GetAllAsync();
        Task<IEnumerable<AcademicProgram>> GetByCollegeAsync(Guid collegeId);
        Task AddAsync(AcademicProgram program);
        Task UpdateAsync(AcademicProgram program);
        Task DeleteAsync(Guid programId);
        Task<dynamic> ExistsAsync(Guid programId);
    }
}
