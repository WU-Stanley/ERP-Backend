using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllRolesAsync();
        }

        public async Task<Role> GetRoleByIdAsync(Guid id)
        {
            return await _roleRepository.GetRoleByIdAsync(id);
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
            return updatedRole != null;
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            return await _roleRepository.AssignUserToRoleAsync(userId, roleId);
        }
        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            return await _roleRepository.RemoveUserFromRoleAsync(userId, roleId);
        }
        public async Task<List<Role>> GetRolesForUserAsync(Guid userId)
        {
            return await _roleRepository.GetRolesForUserAsync(userId);
        }
        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            return await _roleRepository.DeleteRoleAsync(id);
        }
        public async Task<List<User>> GetUsersInRoleAsync(Guid roleId)
        {
            return await _roleRepository.GetUsersInRoleAsync(roleId);
        }
    }
}