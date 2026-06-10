using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private WUIAMDbContext dbContext;

        public PermissionRepository(WUIAMDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<Permission>> GetPermissionsAsync()
        {
            return await dbContext.Permissions.ToListAsync();
        }

        public async Task<Permission> GetPermissionByIdAsync(Guid id)
        {
            var permission = await dbContext.Permissions.FindAsync(id) ?? throw new KeyNotFoundException($"Permission with id {id} not found.");
            return permission;
        }

        public async Task<Permission> AddPermissionAsync(Permission permission)
        {
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission> UpdatePermissionAsync(Guid id, PermissionDto permission)
        {
            var existingPermission = await dbContext.Permissions.FindAsync(id);
            if (existingPermission == null) return null;

            existingPermission.Name = permission.Name;
            existingPermission.Description = permission.Description;

            dbContext.Permissions.Update(existingPermission);
            await dbContext.SaveChangesAsync();
            return existingPermission;
        }

        public async Task<bool> DeletePermissionAsync(Guid id)
        {
            var permission = await dbContext.Permissions.FindAsync(id);
            if (permission != null)
            {
                dbContext.Permissions.Remove(permission);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        // This method checks if a user has a specific permission, either directly or through their roles.
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
        {
            // Check direct user permissions  
            var hasUserPermission = await dbContext.UserPermissions
                .AnyAsync(up => up.UserId == userId && (up.Permission.Name == permission || up.Permission.Name == Permissions.AdminAccess.ToString()));

            if (hasUserPermission)
                return true;

            // Check role-based permissions  
            var hasRolePermission = await dbContext.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserRoles.SelectMany(ur => ur.Role.RolePermissions))
                .AnyAsync(rp => rp.Permission.Name == permission ||rp.Permission.Name =="AdminAccess");

            return hasRolePermission;
        }
        public async Task<bool> RevokePermissionAsync(Guid userId, string permission)
        {
            var userPermission = await dbContext.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission.Name == permission);
            if (userPermission != null)
            {
                dbContext.UserPermissions.Remove(userPermission);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> GrantPermissionAsync(Guid userId, string permission)
        {
            var existingPermission = await dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == permission);
            if (existingPermission == null)
            {
                return false; // Permission does not exist
            }

            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = existingPermission.Id
            };

             await dbContext.UserPermissions.AddAsync(userPermission);
            await dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permission)
        {
            return await dbContext.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permission);
        }
        public async Task<RolePermission?> GrantPermissionToRoleAsync(Guid roleId, string permission)
        {
            var existingPermission = await dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == permission);
            if (existingPermission == null)
            {
                return null; // Permission does not exist
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = existingPermission.Id
            };

            await dbContext.RolePermissions.AddAsync(rolePermission);
            await dbContext.SaveChangesAsync();
            return rolePermission;
        }
        public async Task<bool> RevokePermissionFromRoleAsync(Guid roleId, string permission)
        {
            var rolePermission = await dbContext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permission);
            if (rolePermission != null)
            {
                dbContext.RolePermissions.Remove(rolePermission);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<UserPermissionDto>?> GetUserPermissionsAsync(Guid userId)
        {
            var userPerms = await dbContext.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var permissions = await dbContext.Permissions.ToListAsync();

            var userPermissions = permissions.Select(permission => new UserPermissionDto
            {
                Assigned = userPerms.Any(up => up.PermissionId == permission.Id) ? true : false,
                Description = permission.Description,
                Name = permission.Name,
                Id = permission.Id
            }).ToList();

            return userPermissions;
        }

        public  async Task<List<UserPermissionDto>?> GetRolePermissionsAsync(Guid roleId)
        {
            var rolePerms= await dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleId).ToListAsync();

            var permissions = await dbContext.Permissions.ToListAsync();
            return permissions.Select(rp => new UserPermissionDto
            {
                Id = rp.Id,
                Name = rp.Name,
                Description = rp.Description,
                Assigned = rolePerms.Any(up => up.PermissionId == rp.Id) ? true : false,
            }).ToList();
        }
    }
}

