using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface ICollegeService
    {
        Task<ApiResponse<College>> GetByIdAsync(Guid collegeId);
        Task<ApiResponse<IEnumerable<College>>> GetAllAsync();
        Task<ApiResponse<College>> CreateAsync(College college);
        Task<ApiResponse<College>> UpdateAsync(College college);
        Task<ApiResponse<dynamic>> DeleteAsync(Guid collegeId);
    }
}
