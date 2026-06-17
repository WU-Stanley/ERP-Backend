using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WUIAM.Attributes;
using WUIAM.Enums;
using WUIAM.Interfaces;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly Permissions[] _permissions;

    public HasPermissionAttribute(params Permissions[] permissions)
    {
        _permissions = permissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowsAuthenticatedUsers = context.ActionDescriptor.EndpointMetadata
            .Any(metadata => metadata is AllowAuthenticatedUsersAttribute);

        if (allowsAuthenticatedUsers)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
                return;

            context.Result = new JsonResult(new
            {
                Message = "You must be signed in to access the requested resource"
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        var userIdClaim = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid parsedUserId))
        {
            context.Result = new JsonResult(new
            {
                Message = "You are not authorized to access the requested resource"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        if (IsSuperAdmin(context.HttpContext.User))
        {
            return;
        }

        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        foreach (var permission in _permissions)
        {
            var hasPermission = await permissionService.UserHasPermissionAsync(parsedUserId, permission.ToString());
            if (hasPermission)
            {
                return; // User has at least one required permission: allow access.
            }
        }

        // None of the required permissions were found
        context.Result = new JsonResult(new
        {
            Message = "You are not authorized to access the requested resource"
        })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private static bool IsSuperAdmin(ClaimsPrincipal? user)
    {
        return user?.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role" || claim.Type == "roles")
            .Select(claim => NormalizeRoleName(claim.Value))
            .Any(role => role is "superadmin" or "superadministrator") == true;
    }

    private static string NormalizeRoleName(string roleName)
    {
        return new string(roleName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
