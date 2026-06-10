using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task<Role> CreateRoleAsync(RoleCreateDto roleCreateDto);
        Task<bool> UpdateRoleAsync(Guid roleId, RoleUpdateDto roleUpdateDto);
        Task<bool> DeleteRoleAsync(Guid roleId);
        Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId);
        Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
        Task<List<Role>> GetRolesForUserAsync(Guid userId);
        Task<List<User>> GetUsersInRoleAsync(Guid roleId);
    }

}