using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using WUIAM.Services;
using WUIAM.Models;
using WUIAM.Interfaces;
using WUIAM.DTOs;
using WUIAM.Enums;
using Microsoft.AspNetCore.Authorization;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [HasPermission([Permissions.ManagePermissions])]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // GET: api/Permission

         
        [HttpGet]
       
        public async Task<ActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            //return Ok(permissions);
            if (permissions !=null)
                 return Ok(ApiResponse<IEnumerable<Permission>>.Success("Permissions found!",permissions));

            return Ok(ApiResponse<IEnumerable<Permission>>.Success("No Permission found!", permissions));
        }

        // GET: api/Permission/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Permission>>> GetById(Guid id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
                return NotFound();
            return Ok(ApiResponse<Permission>.Success("Permission found!", permission));
        }

        // POST: api/Permission
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Permission>>> Create([FromBody] PermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _permissionService.AddPermissionAsync(dto);
            return Ok( ApiResponse<Permission>.Success("Permission created successfully!", created));
        }

        // PUT: api/Permission/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Permission>>> Update(Guid id, [FromBody] PermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _permissionService.UpdatePermissionAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpGet("UserPermissions/{userId}")]
        public async Task<ActionResult<ApiResponse<List<UserPermissionDto>>>> GetUserPermissions(Guid userId)
        {
            var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
            if(userPermissions.Success)
            {
                return Ok(ApiResponse<IEnumerable<UserPermissionDto>>.Success(message:"user permissions found!",data:userPermissions.Data!));
            }
            return BadRequest("Error getting user permissions");
        }
        [HttpGet("RolePermissions/{roleId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserPermissionDto>>>> GetRolePermissions(Guid roleId)
        {
            var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);
            if(rolePermissions.Success && rolePermissions.Data != null)
            {
                return Ok(ApiResponse<IEnumerable<UserPermissionDto>>.Success(message:"role permissions found!",data:rolePermissions.Data!));
            }
            return BadRequest(ApiResponse<IEnumerable<UserPermissionDto>>.Failure("Error getting role permissions"));
        }

        // DELETE: api/Permission/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _permissionService.DeletePermissionAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        // POST: api/Permission/GrantRolePermission
        [HttpPost("GrantRolePermission")]
        public async Task<ActionResult<ApiResponse<RolePermission>>> GrantRolePermission([FromBody] GrantRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.GrantPermissionToRoleAsync(dto.RoleId, dto.Permission);
            if (!result.Success)
                return BadRequest(ApiResponse<RolePermission>.Failure(message: "Failed to grant role permission")   );

            return Ok(ApiResponse<RolePermission>.Success(message: "Role permission granted successfully.", data: result.Data));
        }
        // POST: api/Permission/RevokeRolePermission
        [HttpPost("RevokeRolePermission")]
        public async Task<ActionResult<ApiResponse<dynamic>>> RevokeRolePermission([FromBody] GrantRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.RevokePermissionFromRoleAsync(dto.RoleId, dto.Permission);
            if (!result.Success)
                return BadRequest(ApiResponse<RolePermission>.Failure(message: "Failed to revoke role permission"));

            return Ok(ApiResponse<RolePermission>.Success(message: "Role permission revoked successfully.", data: result.Data));
        }
        // POST: api/Permission/HasRolePermission
        [HttpPost("RoleHasPermission")]
        public async Task<ActionResult<ApiResponse<dynamic>>> RoleHasPermission([FromBody] GrantRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.RoleHasPermissionAsync(dto.RoleId, dto.Permission);
            if (!result)
                return NotFound();

            return Ok(ApiResponse<dynamic>.Success(message: "Role has permission", data: result));
        }
        // POST: api/Permission/GrantPermission
        [HttpPost("GrantPermissionToUser")]
        public async Task<ActionResult<ApiResponse<dynamic>>> GrantPermissionToUser([FromBody] GrantPermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.GrantPermissionToUserAsync(dto.UserId, dto.Permission);
            if (result.Success)
            {
                return Ok(ApiResponse<dynamic>.Success(message: "user permissions found!", data: result));
            }
            return Ok(ApiResponse<dynamic>.Failure(message: "user permissions found!"));


        }
        // POST: api/Permission/RevokePermission
        [HttpPost("RevokePermissionFromUser")]
        public async Task<IActionResult> RevokePermissionFromUser([FromBody] GrantPermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.RevokePermissionFromUserAsync(dto.UserId, dto.Permission);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(message: "user permissions found!"));
            return Ok(ApiResponse<dynamic>.Success(message: "user permissions found!", data: result));
        }
        // POST: api/Permission/HasPermission
        [HttpPost("UserHasPermission")]
        public async Task<IActionResult> UserHasPermission([FromBody] GrantPermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.UserHasPermissionAsync(dto.UserId, dto.Permission);
            if (!result)
                return NotFound();

            return Ok(result);
        }

    }
}