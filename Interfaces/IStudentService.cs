using WUIAM.DTOs;
using WUIAM.Models;
namespace WUIAM.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResponse<IEnumerable<Student>>> GetAllStudentsAsync();
        Task<ApiResponse<Student>> GetStudentByIdAsync(Guid studentId);
        Task<ApiResponse<Student>> GetStudentByUserIdAsync(Guid userId);
        Task<ApiResponse<dynamic>> CreateStudentAsync(Student student);
        Task<ApiResponse<dynamic>> UpdateStudentAsync(Student student);
        Task<ApiResponse<dynamic>> DeleteStudentAsync(Guid studentId);

        Task<ApiResponse<IEnumerable<Student>>> GetStudentsByCollegeAsync(Guid collegeId);
        Task<ApiResponse<IEnumerable<Student>>> GetStudentsByProgramAsync(Guid programId);

        // Special business logic
        Task<ApiResponse<dynamic>> SwitchStudentProgramAsync(Guid studentId, Guid newProgramId);
    }
}
