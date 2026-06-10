using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.Interfaces;
using WUIAM.Interfaces;

namespace WUIAM.Services.Implementations
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _collegeRepository;

        public CollegeService(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }

        public async Task<ApiResponse<College>> GetByIdAsync(Guid collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);
            if (college == null)
                return ApiResponse<College>.Failure("College not found.");

            return ApiResponse<College>.Success("College retrieved successfully.", college);
        }

        public async Task<ApiResponse<IEnumerable<College>>> GetAllAsync()
        {
            var colleges = await _collegeRepository.GetAllAsync();
            return ApiResponse<IEnumerable<College>>.Success("Colleges retrieved successfully.", colleges);
        }

        public async Task<ApiResponse<College>> CreateAsync(College college)
        {
            if (await _collegeRepository.ExistsAsync(college.CollegeId))
                return ApiResponse<College>.Failure("College already exists.");

            await _collegeRepository.AddAsync(college);
            return ApiResponse<College>.Success("College created successfully.", college);
        }

        public async Task<ApiResponse<College>> UpdateAsync(College college)
        {
            var exists = await _collegeRepository.ExistsAsync(college.CollegeId);
            if (!exists)
                return ApiResponse<College>.Failure("College not found.");

            await _collegeRepository.UpdateAsync(college);
            return ApiResponse<College>.Success("College updated successfully.", college);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(Guid collegeId)
        {
            var exists = await _collegeRepository.ExistsAsync(collegeId);
            if (!exists)
                return ApiResponse<dynamic>.Failure("College not found.");

            await _collegeRepository.DeleteAsync(collegeId);
            return ApiResponse<dynamic>.Success("College deleted successfully.", true);
        }
    }
}
