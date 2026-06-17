using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    /// <summary>
    /// Role service with memory caching for frequently-read role data.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMemoryCache _cache;

        private const string CacheKeyAll = "roles_all";

        public RoleService(IRoleRepository roleRepository, IMemoryCache cache)
        {
            _roleRepository = roleRepository;
            _cache = cache;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _cache.GetOrCreateStatic(CacheKeyAll, async () => await _roleRepository.GetAllRolesAsync())
                ?? await _roleRepository.GetAllRolesAsync();
        }

        public async Task<Role> GetRoleByIdAsync(Guid id)
        {
            var cacheKey = $"role_{id}";
            return await _cache.GetOrCreateStatic(cacheKey, async () => await _roleRepository.GetRoleByIdAsync(id))
                ?? await _roleRepository.GetRoleByIdAsync(id);
        }

        public async Task<Role> CreateRoleAsync(RoleCreateDto role)
        {
            var newRole = new Role
            {
                Name = role.Name,
                Description = role.Description
            };
            var createdRole = await _roleRepository.AddRoleAsync(newRole);
            if (createdRole == null)
            {
                throw new System.Exception("Failed to create role.");
            }
            _cache.Invalidate(CacheKeyAll);
            return createdRole;
        }

        public async Task<bool> UpdateRoleAsync(Guid id, RoleUpdateDto roleUpdateDto)
        {
            var existingRole = await _roleRepository.GetRoleByIdAsync(id);
            if (existingRole == null)
            {
                throw new KeyNotFoundException($"Role with Id {id} not found.");
            }

            existingRole.Name = roleUpdateDto.Name;
            existingRole.Description = roleUpdateDto.Description;

            var updatedRole = await _roleRepository.UpdateRoleAsync(existingRole);
            _cache.Invalidate(CacheKeyAll);
            _cache.Invalidate($"role_{id}");
            return updatedRole != null;
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            var result = await _roleRepository.AssignUserToRoleAsync(userId, roleId);
            _cache.Invalidate(CacheKeyAll);
            return result;
        }
        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            var result = await _roleRepository.RemoveUserFromRoleAsync(userId, roleId);
            _cache.Invalidate(CacheKeyAll);
            return result;
        }
        public async Task<List<Role>> GetRolesForUserAsync(Guid userId)
        {
            var cacheKey = $"user_roles_{userId}";
            return await _cache.GetOrCreateDynamic(cacheKey, async () => await _roleRepository.GetRolesForUserAsync(userId))
                ?? await _roleRepository.GetRolesForUserAsync(userId);
        }
        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            var result = await _roleRepository.DeleteRoleAsync(id);
            _cache.Invalidate(CacheKeyAll);
            _cache.Invalidate($"role_{id}");
            return result;
        }
        public async Task<List<User>> GetUsersInRoleAsync(Guid roleId)
        {
            var cacheKey = $"role_users_{roleId}";
            return await _cache.GetOrCreateStatic(cacheKey, async () => await _roleRepository.GetUsersInRoleAsync(roleId))
                ?? await _roleRepository.GetUsersInRoleAsync(roleId);
        }
    }
}
