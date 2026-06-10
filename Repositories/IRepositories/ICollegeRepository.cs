using WUIAM.Models;

namespace WUIAM.Repositories.Interfaces
{
    public interface ICollegeRepository
    {
        Task<College?> GetByIdAsync(Guid collegeId);
        Task<IEnumerable<College>> GetAllAsync();
        Task AddAsync(College college);
        Task UpdateAsync(College college);
        Task DeleteAsync(Guid collegeId);
        Task<bool> ExistsAsync(Guid collegeId);
    }
}
