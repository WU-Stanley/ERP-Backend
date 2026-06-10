using System.Xml;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories;
using WUIAM.Repositories.Interfaces;
using WUIAM.Repositories.IRepositories;


namespace WUIAM.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IProgramRepository _programRepository; // to validate new program
        private readonly WUIAMDbContext _context; // for cross-entity updates if needed

        public StudentService(
            IStudentRepository studentRepository,
            IProgramRepository programRepository,
            WUIAMDbContext context)
        {
            _studentRepository = studentRepository;
            _programRepository = programRepository;
            _context = context;
        }

        public async Task<ApiResponse<IEnumerable<Student>>> GetAllStudentsAsync()
        {
            var students = await _studentRepository.GetAllAsync();
            return ApiResponse<IEnumerable<Student>>.Success("Students retrieved successfully.", students);
        }

        public async Task<ApiResponse<Student>> GetStudentByIdAsync(Guid studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return ApiResponse<Student>.Failure("Student not found.");

            return ApiResponse<Student>.Success("Student retrieved successfully.", student);
        }

        public async Task<ApiResponse<Student>> GetStudentByUserIdAsync(Guid userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
                return ApiResponse<Student>.Failure("Student not found.");

            return ApiResponse<Student>.Success("Student retrieved successfully.", student);
        }

        public async Task<ApiResponse<dynamic>> CreateStudentAsync(Student student)
        {
            await _studentRepository.AddAsync(student);
            return ApiResponse<dynamic>.Success("Student created successfully.", true);
        }

        public async Task<ApiResponse<dynamic>> UpdateStudentAsync(Student student)
        {
            await _studentRepository.UpdateAsync(student);
            return ApiResponse<dynamic>.Success("Student updated successfully.", true);
        }

        public async Task<ApiResponse<dynamic>> DeleteStudentAsync(Guid studentId)
        {
            await _studentRepository.DeleteAsync(studentId);
            return ApiResponse<dynamic>.Success("Student deleted successfully.", true);
        }

        public async Task<ApiResponse<IEnumerable<Student>>> GetStudentsByCollegeAsync(Guid collegeId)
        {
            var students = await _studentRepository.GetByCollegeIdAsync(collegeId);
            return ApiResponse<IEnumerable<Student>>.Success("Students by college retrieved successfully.", students);
        }

        public async Task<ApiResponse<IEnumerable<Student>>> GetStudentsByProgramAsync(Guid programId)
        {
            var students = await _studentRepository.GetByProgramIdAsync(programId);
            return ApiResponse<IEnumerable<Student>>.Success("Students by program retrieved successfully.", students);
        }

        public async Task<ApiResponse<dynamic>> SwitchStudentProgramAsync(Guid studentId, Guid newProgramId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                    return ApiResponse<dynamic>.Failure("Student not found.");

                var newProgram = await _programRepository.GetByIdAsync(newProgramId);
                if (newProgram == null)
                    return ApiResponse<dynamic>.Failure("Target program not found.");

                // update both program and college
                student.ProgramId = newProgram.ProgramId;
                student.CollegeId = newProgram.CollegeId;

                await _studentRepository.UpdateAsync(student);

                // Commit transaction if all goes well
                await transaction.CommitAsync();

                return ApiResponse<dynamic>.Success("Student switched to new program successfully.", true);
            }
            catch (Exception ex)
            {
                // Rollback everything if an error occurs
                await transaction.RollbackAsync();
                return ApiResponse<dynamic>.Failure("Failed to switch program.", ex.Message);
            }
        }

    }
}
