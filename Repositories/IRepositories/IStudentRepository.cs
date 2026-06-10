using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(Guid studentId);
        Task<Student?> GetByUserIdAsync(Guid userId);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Guid studentId);

        // Extra useful queries
        Task<IEnumerable<Student>> GetByCollegeIdAsync(Guid collegeId);
        Task<IEnumerable<Student>> GetByProgramIdAsync(Guid programId);
    }
}
