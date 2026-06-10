using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IAcademicProgramService
    {
        Task<ApiResponse<AcademicProgram>> GetByIdAsync(Guid programId);
        Task<ApiResponse<IEnumerable<AcademicProgram>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<AcademicProgram>>> GetByCollegeAsync(Guid collegeId);
        Task<ApiResponse<AcademicProgram>> CreateAsync(AcademicProgram program);
        Task<ApiResponse<AcademicProgram>> UpdateAsync(AcademicProgram program);
        Task<ApiResponse<dynamic>> DeleteAsync(Guid programId);
    }
}
