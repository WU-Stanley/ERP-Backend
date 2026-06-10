using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<Role?> AddRoleAsync(Role role);
        Task<Role?> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(Guid roleId);
        Task<bool> RoleExistsAsync(Guid roleId);
        Task<List<User>> GetUsersInRoleAsync(Guid roleId);
        Task<bool> AssignUserToRoleAsync(Guid userId, Guid roleId);
        Task<bool> RemoveUserFromRoleAsync(Guid userId, Guid roleId);
        Task<List<Role>> GetRolesForUserAsync(Guid userId);
    }
}