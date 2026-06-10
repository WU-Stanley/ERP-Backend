using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
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
}
