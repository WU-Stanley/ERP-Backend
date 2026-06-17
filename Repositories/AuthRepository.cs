using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    /// <summary>
    /// Optimized repository with query projection, compiled queries, and efficient includes.
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly WUIAMDbContext _dbContext;

        // Compiled query for user lookup by email (avoids re-compiling the same query)
        private static readonly Func<WUIAMDbContext, string, Task<User?>> _findUserByEmail =
            EF.CompileAsyncQuery((WUIAMDbContext db, string email) =>
                db.Users.FirstOrDefault(u => u.UserEmail == email && !u.IsDeleted));

        // Compiled query for user lookup by email or username
        private static readonly Func<WUIAMDbContext, string, Task<User?>> _findUserByEmailOrName =
            EF.CompileAsyncQuery((WUIAMDbContext db, string email) =>
                db.Users.FirstOrDefault(u => (u.UserEmail == email || u.UserName == email) && !u.IsDeleted));

        // Compiled query for user lookup by ID with all auth-related navigations
        private static readonly Func<WUIAMDbContext, Guid, Task<User?>> _findUserByIdWithAuth =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId) =>
                db.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .Include(u => u.UserRoles.Where(ur => ur.Role != null))
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions.Where(rp => rp.Permission != null))
                                .ThenInclude(rp => rp.Permission)
                    .Include(u => u.UserPermissions.Where(up => up.Permission != null))
                        .ThenInclude(up => up.Permission)
                    .Include(u => u.Employee)
                        .ThenInclude(e => e.Employments.Where(e => e.IsActive))
                            .ThenInclude(d => d.Department)
                    .FirstOrDefault());

        // Compiled query for finding user by email with all auth navigations
        private static readonly Func<WUIAMDbContext, string, Task<User?>> _findUserByEmailWithAuth =
            EF.CompileAsyncQuery((WUIAMDbContext db, string email) =>
                db.Users
                    .Where(u => (u.UserEmail == email || u.UserName == email) && !u.IsDeleted)
                    .Include(u => u.UserRoles.Where(ur => ur.Role != null))
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions.Where(rp => rp.Permission != null))
                                .ThenInclude(rp => rp.Permission)
                    .Include(u => u.UserPermissions.Where(up => up.Permission != null))
                        .ThenInclude(up => up.Permission)
                    .Include(u => u.Employee)
                        .ThenInclude(e => e.Employments.Where(e => e.IsActive))
                            .ThenInclude(d => d.Department)
                    .FirstOrDefault());

        // Compiled query for latest MFA token
        private static readonly Func<WUIAMDbContext, Guid, Task<MFAToken?>> _getLatestToken =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId) =>
                db.MFATokens
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault());

        // Compiled query for active refresh tokens
        private static readonly Func<WUIAMDbContext, Guid, Task<List<RefreshToken>>> _getActiveTokens =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId) =>
                db.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedOn == null && rt.IsExpired == false)
                    .ToList());

        // Compiled query for users by role
        private static readonly Func<WUIAMDbContext, string, Task<List<UserDto>>> _getUsersByRole =
            EF.CompileAsyncQuery((WUIAMDbContext db, string roleId) =>
                db.Users
                    .Where(u => !u.IsDeleted && u.UserRoles.Any(ur => ur.RoleId.ToString() == roleId))
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.UserEmail,
                        UserTypeId = u.UserTypeId,
                    })
                    .ToList());

        public AuthRepository(WUIAMDbContext context)
        {
            _dbContext = context;
        }

        public async Task ExpireTwoFactorTokenAsync(Guid id)
        {
            var token = await _dbContext.MFATokens.FindAsync(id);
            if (token != null)
            {
                _dbContext.MFATokens.Remove(token);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkTwoFactorTokenAsUsedAsync(Guid id)
        {
            var token = await _dbContext.MFATokens.FindAsync(id);
            if (token != null && !token.IsUsed)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task<User?> FindUserByEmailAsync(string email)
        {
            return _findUserByEmail(_dbContext, email);
        }

        public Task<User?> FindUserByEmailOrUserNameAsync(string email)
        {
            return _findUserByEmailWithAuth(_dbContext, email);
        }

        public Task<User?> FindUserByIdAsync(Guid userId)
        {
            return _findUserByIdWithAuth(_dbContext, userId);
        }

        public async Task<MFAToken?> GetLatestTwoFactorTokenAsync(Guid userId)
        {
            return await _getLatestToken(_dbContext, userId);
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            var s = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public Task<User> UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
            return _dbContext.SaveChangesAsync().ContinueWith(t => user);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task ExpireRefreshTokenAsync(RefreshToken refreshToken, string? replacedByToken = null, string? revokedByIp = null)
        {
            refreshToken.RevokedOn = DateTime.UtcNow;
            refreshToken.ReplacedByToken = replacedByToken;
            refreshToken.RevokedByIp = revokedByIp;
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserType?>> getUserTypes()
        {
            return await _dbContext.UserTypes.ToListAsync();
        }

        public async Task<int> SaveTwoFactorTokenAsync(Guid userId, string twoFactorToken)
        {
            var token = new MFAToken
            {
                UserId = userId,
                Token = twoFactorToken,
                ClientId = "WUIAM",
                ExpiresOn = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _dbContext.MFATokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();
            return 1;
        }

        public async Task<IEnumerable<UserDto?>?> GetStaffListAsync()
        {
            var staffType = await _dbContext.UserTypes
                .FirstOrDefaultAsync(ut => ut.Name.ToLower().Contains("staff"));

            if (staffType == null)
                return null;

            // Optimized: single query, no unnecessary includes
            return await _dbContext.Users
                .Where(u => u.UserTypeId == staffType.Id && !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    EmployeeId = u.Employee != null ? u.Employee.EmployeeId : null,
                    FullName = u.FullName,
                    Email = u.UserEmail!,
                    UserTypeId = u.UserTypeId,
                    DepartmentId = u.Employee != null
                        ? u.Employee.Employments
                            .Where(e => e.IsActive)
                            .Select(e => e.DepartmentId)
                            .FirstOrDefault()
                        : null,
                })
                .ToListAsync();
        }

        public Task<List<UserDto>> GetUsersByRoleAsync(string approverValue)
        {
            if (string.IsNullOrWhiteSpace(approverValue))
                return Task.FromResult(new List<UserDto>());

            return _getUsersByRole(_dbContext, approverValue);
        }

        public async Task<IEnumerable<EmploymentType>> GetEmploymentTypes()
        {
            return await _dbContext.EmploymentTypes.ToListAsync();
        }

        public Task<UserType> CreateUserTypeAsync(UserType userType)
        {
            _dbContext.UserTypes.Add(userType);
            return _dbContext.SaveChangesAsync().ContinueWith(t => userType);
        }

        public async Task<EmploymentType> CreateEmploymentTypeAsync(EmploymentType employmentType)
        {
            await _dbContext.EmploymentTypes.AddAsync(employmentType);
            await _dbContext.SaveChangesAsync();
            return employmentType;
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensByUserIdAsync(Guid userId)
        {
            return await _getActiveTokens(_dbContext, userId);
        }

        public async Task UpdateRefreshTokensAsync(IEnumerable<RefreshToken> tokens)
        {
            _dbContext.RefreshTokens.UpdateRange(tokens);
            await _dbContext.SaveChangesAsync();
        }
    }
}
