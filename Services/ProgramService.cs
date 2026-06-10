using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.Interfaces;
using WUIAM.Interfaces;

namespace WUIAM.Services.Implementations
{
    public class AcademicProgramService : IAcademicProgramService
    {
        private readonly IProgramRepository _programRepository;

        public AcademicProgramService(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        public async Task<ApiResponse<AcademicProgram>> GetByIdAsync(Guid programId)
        {
            var program = await _programRepository.GetByIdAsync(programId);
            if (program == null)
                return ApiResponse<AcademicProgram>.Failure("Academic program not found.");

            return ApiResponse<AcademicProgram>.Success("Academic program retrieved successfully.", program);
        }

        public async Task<ApiResponse<IEnumerable<AcademicProgram>>> GetAllAsync()
        {
            var programs = await _programRepository.GetAllAsync();
            return ApiResponse<IEnumerable<AcademicProgram>>.Success("Academic programs retrieved successfully.", programs);
        }

        public async Task<ApiResponse<IEnumerable<AcademicProgram>>> GetByCollegeAsync(Guid collegeId)
        {
            var programs = await _programRepository.GetByCollegeAsync(collegeId);
            return ApiResponse<IEnumerable<AcademicProgram>>.Success("Academic programs retrieved successfully by college.", programs);
        }

        public async Task<ApiResponse<AcademicProgram>> CreateAsync(AcademicProgram program)
        {
            if (await _programRepository.ExistsAsync(program.ProgramId))
                return ApiResponse<AcademicProgram>.Failure("Academic program already exists.");

            await _programRepository.AddAsync(program);
            return ApiResponse<AcademicProgram>.Success("Academic program created successfully.", program);
        }

        public async Task<ApiResponse<AcademicProgram>> UpdateAsync(AcademicProgram program)
        {
            var exists = await _programRepository.ExistsAsync(program.ProgramId);
            if (!exists)
                return ApiResponse<AcademicProgram>.Failure("Academic program not found.");

            await _programRepository.UpdateAsync(program);
            return ApiResponse<AcademicProgram>.Success("Academic program updated successfully.", program);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(Guid programId)
        {
            var exists = await _programRepository.ExistsAsync(programId);
            if (!exists)
                return ApiResponse<dynamic>.Failure("Academic program not found.");

            await _programRepository.DeleteAsync(programId);
            return ApiResponse<dynamic>.Success("Academic program deleted successfully.", true);
        }
    }
}
