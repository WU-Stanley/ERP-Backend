using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IAuthRepository
    {
        Task ExpireTwoFactorTokenAsync(Guid id);
        public Task<User?> FindUserByEmailAsync(string Email);

        public Task<User?> FindUserByEmailOrUserNameAsync(string Email);
        public Task<User?> FindUserByIdAsync(Guid userId);
        public Task<MFAToken?> GetLatestTwoFactorTokenAsync(Guid userId);
        public Task<User> RegisterUserAsync(User user);
        public Task<int> SaveTwoFactorTokenAsync(Guid userId, string twoFactorToken);
        public Task<User> UpdateUserAsync(User user);
        public Task<RefreshToken?> GetRefreshTokenAsync(string token);
        public Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
        void ExpireRefreshTokenAsync(RefreshToken token);
        Task<IEnumerable<UserType?>> getUserTypes();
        Task<IEnumerable<UserDto?>?> GetStaffListAsync();
        Task<List<UserDto>> GetUsersByRoleAsync(string approverValue);
        Task<IEnumerable<EmploymentType>> GetEmploymentTypes();
        Task<UserType> CreateUserTypeAsync(UserType userType);
        Task<EmploymentType> CreateEmploymentTypeAsync(EmploymentType employmentType);
    }
}
