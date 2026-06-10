using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Repositories.IRepositories
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetPermissionsAsync();
        Task<Permission> GetPermissionByIdAsync(Guid id);
        Task<Permission> AddPermissionAsync(Permission permission);
        Task<Permission> UpdatePermissionAsync(Guid id, PermissionDto permission);
        Task<bool> DeletePermissionAsync(Guid id);
        Task<bool> UserHasPermissionAsync(Guid userId, string permission);
        Task<bool> RevokePermissionAsync(Guid userId, string permission);
        Task<bool> GrantPermissionAsync(Guid userId, string permission);
        Task<bool> RoleHasPermissionAsync(Guid roleId, string permission);
        Task<RolePermission?> GrantPermissionToRoleAsync(Guid roleId, string permission);
        Task<bool> RevokePermissionFromRoleAsync(Guid roleId, string permission);
        Task<List<UserPermissionDto>?> GetUserPermissionsAsync(Guid userId);
        Task<List<UserPermissionDto>?> GetRolePermissionsAsync(Guid roleId);
    }
}