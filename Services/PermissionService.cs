using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{


    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        // This is a placeholder for your data store (e.g., database, in-memory, etc.) 
        public Task<bool> UserHasPermissionAsync(Guid userId, string permission)
        {
            return _permissionRepository.UserHasPermissionAsync(userId, permission);
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _permissionRepository.GetPermissionsAsync();
        }

        public async Task<(bool Success, string Message, dynamic? Data)> GrantPermissionToUserAsync(Guid userId, string permission)
        {
            var result = await _permissionRepository.GrantPermissionAsync(userId, permission);
            if (result)
            {
                return (true, "Permission granted successfully.", result);
            }
            return (false, "Failed to grant permission.", null);
        }

        public async Task<(bool Success, string Message, dynamic? Data)> RevokePermissionFromUserAsync(Guid userId, string permission)
        {
            var result = await _permissionRepository.RevokePermissionAsync(userId, permission);
            if (result)
            {
                return (true, "Permission revoked successfully.", result);
            }
            return (false, "Failed to revoke permission.", null);
        }


        public async Task<Permission> GetPermissionByIdAsync(Guid id)
        {
            return await _permissionRepository.GetPermissionByIdAsync(id);
        }

        public async Task<Permission> AddPermissionAsync(PermissionDto permission)
        {
            var newPermission = new Permission
            {
                Name = permission.Name,
                Description = permission.Description
            };
            return await _permissionRepository.AddPermissionAsync(newPermission);
        }

        public async Task<Permission> UpdatePermissionAsync(Guid id, PermissionDto permission)
        {
           
            return await _permissionRepository.UpdatePermissionAsync(id, permission);
        }

        public async Task<bool> DeletePermissionAsync(Guid id)
        {
            return await _permissionRepository.DeletePermissionAsync(id);
        }
        public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permission)
        {
            return await _permissionRepository.RoleHasPermissionAsync(roleId, permission);
        }
        public async Task<(bool Success, string Message, RolePermission? Data)> GrantPermissionToRoleAsync(Guid roleId, string permission)
        {
            var result = await _permissionRepository.GrantPermissionToRoleAsync(roleId, permission);
            if (result != null)
            {
                return (true, "Role permission granted successfully.", result);
            }
            return (false, "Failed to grant role permission.", null);
        }
        public async Task<(bool Success, string Message, dynamic? Data)> RevokePermissionFromRoleAsync(Guid roleId, string permission)
        {
            var result = await _permissionRepository.RevokePermissionFromRoleAsync(roleId, permission);
            if (result)
            {
                return (true, "Role permission revoked successfully.", null);
            }
            return (false, "Failed to revoke role permission.", null);
        }

        public async Task<(bool Success, string Message, IEnumerable<UserPermissionDto>? Data)> GetUserPermissionsAsync(Guid userId)
        {
            if (userId != null)
            {
                var userPermissions = await _permissionRepository.GetUserPermissionsAsync(userId);
                if  (userPermissions != null)
                {
                    return (true, "User permissions found", userPermissions);
                }
                return (false, "No user permission found", null);
            }
            return (false, "No user permission found", null);
        }

        public async Task<(bool Success, string Message, IEnumerable<UserPermissionDto?> Data)> GetRolePermissionsAsync(Guid roleId)
        {
           if (roleId !=null)
            {
                var rolePermissions = await _permissionRepository.GetRolePermissionsAsync(roleId);
                if(rolePermissions != null)
                {
                    return (true, "Role permissions found", rolePermissions) ;
                }
                return (false, "No role permission found", null);
            }
            return (false, "No role permission found", null);
        }
    }
}