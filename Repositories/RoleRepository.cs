using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly WUIAMDbContext _context;

        public RoleRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions).Include(r => r.UserRoles) // If you want to include RolePermissions
                .ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(Guid id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions).Include(r => r.UserRoles) // If you want to include RolePermissions
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                throw new KeyNotFoundException($"Role with Id {id} not found.");
            }

            return role;
        }
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions) // If you want to include RolePermissions
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                throw new KeyNotFoundException($"Role with Name {roleName} not found.");
            }

            return role;
        }

        public async Task<Role?> AddRoleAsync(Role role)
        {
            var saved = _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return saved.Entity;
        }

        public async Task<Role?> UpdateRoleAsync(Role role)
        {
            var updated = _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return updated.Entity;
        }

        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public Task<bool> RoleExistsAsync(Guid roleId)
        {
            return _context.Roles.AnyAsync(r => r.Id == roleId);
        }

        public Task<List<User>> GetUsersInRoleAsync(Guid roleId)
        {
            return _context.UserRoles
                .Where(u => u.RoleId == roleId)
                .Select(u => u.User)
                .ToListAsync();
        }

        public async Task<bool> AssignUserToRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = new UserRole { UserId = userId, RoleId = roleId };
            _context.UserRoles.Add(userRole);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserFromRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public Task<List<Role>> GetRolesForUserAsync(Guid userId)
        {
            var roles =  _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
            return roles;
        }
    }
}