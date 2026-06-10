using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Services;
using WUIAM.Models;
using WUIAM.Interfaces;
using WUIAM.DTOs;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Role>>>> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<IEnumerable<Role>>.Success("Roles found", roles));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Role>>> GetRole(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(ApiResponse<Role>.Failure("Role not found"));
            return Ok(ApiResponse<Role>.Success("Role found", role));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Role>>> CreateRole([FromBody] RoleCreateDto role)
        {
            var createdRole = await _roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, ApiResponse<Role>.Success("Role created", createdRole));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleUpdateDto role)
        {
            var updated = await _roleService.UpdateRoleAsync(id, role);
            if (!updated)
                return NotFound();

            return Ok(new { Message = "Role updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var deleted = await _roleService.DeleteRoleAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Role deleted successfully." });
        }

        [HttpPost("assign/{userId}/{roleId}")]
        public async Task<ActionResult<ApiResponse<dynamic>>> AssignRoleToUser(Guid userId, Guid roleId)
        {
            var assigned = await _roleService.AssignRoleToUserAsync(userId, roleId);
            if (!assigned)
                return NotFound(ApiResponse<dynamic>.Failure("Role assignment failed"));

            return Ok(ApiResponse<dynamic>.Success("Role assigned to user successfully.", assigned));
        }

        [HttpDelete("remove/{userId}/{roleId}")]
        public async Task<ActionResult<ApiResponse<dynamic>>> RemoveRoleFromUser(Guid userId, Guid roleId)
        {
            var removed = await _roleService.RemoveRoleFromUserAsync(userId, roleId);
            if (!removed)
                return NotFound(ApiResponse<dynamic>.Failure("Role removal failed"));

            return Ok(ApiResponse<dynamic>.Success("Role removed from user successfully.", removed));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Role>>>> GetRolesForUser(Guid userId)
        {
            var roles = await _roleService.GetRolesForUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<Role>>.Success("Roles found", roles));
        }
        
        [HttpGet("users/{roleId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetUsersInRole(Guid roleId)
        {
            var users = await _roleService.GetUsersInRoleAsync(roleId);
            if (users == null || users.Count == 0)
                return NotFound(ApiResponse<IEnumerable<User>>.Failure("No users found in role"));

            return Ok(ApiResponse<IEnumerable<User>>.Success("Users found in role", users));
        }

    }
}