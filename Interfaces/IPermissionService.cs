using WUIAM.DTOs;
using WUIAM.Models;

public interface IPermissionService
{
    /// <summary>
    /// Retrieves all permissions asynchronously.
    /// </summary>
    /// <returns>A IEnumerable of all <see cref="Permission"/> objects.</returns>
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();

    /// <summary>
    /// Retrieves a permission by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the permission.</param>
    /// <returns>The <see cref="Permission"/> object if found; otherwise, null.</returns>
    Task<Permission?> GetPermissionByIdAsync(Guid id);

    /// <summary>
    /// Adds a new permission asynchronously.
    /// </summary>
    /// <param name="permission">The data transfer object containing permission details.</param>
    /// <returns>The created <see cref="Permission"/> object.</returns>
    Task<Permission> AddPermissionAsync(PermissionDto permission);

    /// <summary>
    /// Updates an existing permission asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the permission to update.</param>
    /// <param name="permission">The data transfer object containing updated permission details.</param>
    /// <returns>The updated <see cref="Permission"/> object.</returns>
    Task<Permission?> UpdatePermissionAsync(Guid id, PermissionDto permission);

    /// <summary>
    /// Deletes a permission by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the permission to delete.</param>
    /// <returns><c>true</c> if the permission was deleted; otherwise, <c>false</c>.</returns>
    Task<bool> DeletePermissionAsync(Guid id);

    /// <summary>
    /// Checks if a user has a specific permission, either directly or through their roles, asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permission">The name of the permission to check.</param>
    /// <returns><c>true</c> if the user has the permission; otherwise, <c>false</c>.</returns>
    Task<bool> UserHasPermissionAsync(Guid userId, string permission);

    /// <summary>
    /// Grants a specific permission to a user through their role asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permission">The name of the permission to grant.</param>
    /// <returns>A tuple indicating success, a message, and optional data.</returns>
    Task<(bool Success, string Message, object? Data)> GrantPermissionToUserAsync(Guid userId, string permission);

    /// <summary>
    /// Revokes a specific permission from a user through their role asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permission">The name of the permission to revoke.</param>
    /// <returns>A tuple indicating success, a message, and optional data.</returns>
    Task<(bool Success, string Message, object? Data)> RevokePermissionFromUserAsync(Guid userId, string permission);

    /// <summary>
    /// Checks if a role has a specific permission asynchronously.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permission">The name of the permission to check.</param>
    /// <returns><c>true</c> if the role has the permission; otherwise, <c>false</c>.</returns>
    Task<bool> RoleHasPermissionAsync(Guid roleId, string permission);

    /// <summary>
    /// Revokes a specific permission from a role asynchronously.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permission">The name of the permission to revoke.</param>
    /// <returns>A tuple indicating success, a message, and optional data.</returns>
    Task<(bool Success, string Message, dynamic? Data)> RevokePermissionFromRoleAsync(Guid roleId, string permission);

    /// <summary>
    /// Grants a specific permission to a role asynchronously.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permission">The name of the permission to grant.</param>
    /// <returns>A tuple indicating success, a message, and optional data.</returns>
    Task<(bool Success, string Message, RolePermission? Data)> GrantPermissionToRoleAsync(Guid roleId, string permission); 
    Task<(bool Success, string Message, IEnumerable<UserPermissionDto>? Data)> GetUserPermissionsAsync(Guid userId);
    Task<(bool Success, string Message, IEnumerable<UserPermissionDto?> Data)> GetRolePermissionsAsync(Guid roleId);
} 