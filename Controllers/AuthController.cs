using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Threading.Tasks;
using WUIAM.Services;
using WUIAM.Models;
using WUIAM.Interfaces;
using WUIAM.DTOs;
using Microsoft.AspNetCore.Authorization;
using WUIAM.Enums;
using System.Security.Claims;


/// <summary>
/// API v1 - Authentication and user management endpoints.
/// </summary>
namespace WUIAM.Controllers
{
    /// <summary>
    /// API v1 - Authentication and user management endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }

        [AllowAnonymous]
        [HttpPost("microsoft-login")]
        public async Task<IActionResult> MicrosoftLogin([FromBody] MicrosoftLoginDto request)
        {
            var result = await _authService.MicrosoftLoginAsync(request);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }

        [HttpPost("impersonate/stop")]
        public async Task<IActionResult> StopImpersonation()
        {
            var impersonatorIdClaim = User.FindFirstValue("ImpersonatorId");
            if (string.IsNullOrWhiteSpace(impersonatorIdClaim) || !Guid.TryParse(impersonatorIdClaim, out Guid impersonatorId))
            {
                return BadRequest(ApiResponse<dynamic>.Failure("Current session is not impersonating another user."));
            }

            var result = await _authService.StopImpersonationAsync(impersonatorId);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }

        [HasPermission(Permissions.ManageUsers, Permissions.SuperAdminAccess, Permissions.AdminAccess)]
        [HttpPost("impersonate/{userId:guid}")]
        public async Task<IActionResult> Impersonate(Guid userId)
        {
            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdClaim) || !Guid.TryParse(adminIdClaim, out Guid adminId))
            {
                return Unauthorized(ApiResponse<dynamic>.Failure("Unauthenticated admin."));
            }

            var result = await _authService.ImpersonateAsync(userId, adminId);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }

        [HasPermission(Permissions.ManageUsers, Permissions.SuperAdminAccess, Permissions.AdminAccess)]
        [HttpPost("impersonate/employee/{employeeId:guid}")]
        public async Task<IActionResult> ImpersonateEmployee(Guid employeeId)
        {
            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdClaim) || !Guid.TryParse(adminIdClaim, out Guid adminId))
            {
                return Unauthorized(ApiResponse<dynamic>.Failure("Unauthenticated admin."));
            }

            var result = await _authService.ImpersonateEmployeeAsync(employeeId, adminId);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }

        [AllowAnonymous]
        [HttpPost("verify-login-token")]
        public async Task<IActionResult> VerifyLoginToken([FromBody] VerifyLoginTokenDto request)
        {
            var result = await _authService.VerifyLoginTokenAsync(request.Email, request.Token);
            if (!result.Success)
                return Ok(ApiResponse<dynamic>.Failure(result.Message));

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(ApiResponse<dynamic>.Success(result.Message, result));
        }
        //[AllowAnonymous]
        [HttpGet("users")]
        public async Task<IActionResult> getUsers()
        {

            var result = await _authService.GetStaffListAsync();
            if (result == null)
                return BadRequest(ApiResponse<object>.Failure("Failed to fetch user types", new { reason = "No user found" }));

            return Ok(ApiResponse<IEnumerable<UserDto?>?>.Success("user types found", result));


        }
        [HasPermission(Permissions.ManageUsers, Permissions.CreateUser)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == null)
                return BadRequest("User registration failed!");
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();


            var result = await _authService.GetRefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Unauthorized(result.Message);

            AppendRefreshTokenCookie(GetStringProperty(result, "refreshToken"));

            return Ok(result);
        }

        //POST: /api/auth/create-user-type
        [HasPermission(Permissions.ManageUsers, Permissions.CreateUser)]
        [HttpPost("create-user-type")]
        public async Task<ActionResult<ApiResponse<UserType>>> CreateUserType([FromBody] UserTypeDto request)
        {
            var result = await _authService.CreateUserTypeAsync(request);
            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        //[AllowAnonymous]
        [HttpGet("get-user-type")]
        public async Task<IActionResult> GetUserTypes()
        {
            var result = await _authService.getUserTypes();
            if (!result.Status)
                return BadRequest(result.message);

            return Ok(result);
        }

        //POST: /api/auth/create-employment-type
        [HasPermission(Permissions.ManageUsers, Permissions.CreateUser)]
        [HttpPost("create-employment-type")]
        public async Task<ActionResult<ApiResponse<EmploymentType>>> CreateEmploymentType([FromBody] EmploymentType request)
        {
            if (request == null)
                return BadRequest(ApiResponse<EmploymentType>.Failure("Invalid employment type data"));

            var result = await _authService.CreateEmploymentTypeAsync(request);
            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("get-employment-types")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmploymentType>>>> GetEmploymentTypes()
        {
            var result = await _authService.GetEmploymentTypes();

            if (result != null)
                return Ok(ApiResponse<IEnumerable<EmploymentType>>.Success("employment types found!", (IEnumerable<EmploymentType>)result));

            return Ok(ApiResponse<IEnumerable<EmploymentType>>.Failure("no employment type found!"));
        }


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTo request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result.status)
                return BadRequest(result.message);

            return Ok(result);
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var result = await _authService.ChangePasswordAsync(request);
            if (!result.status)
                return BadRequest(result.message);
            return Ok(new { message = result.message, status = result.status });
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (string.IsNullOrEmpty(result) || result == "User not found")
                return BadRequest(ApiResponse<dynamic>.Failure(message: "Failed to send reset password email."));

            return Ok(ApiResponse<dynamic>.Success(message: "Reset password email sent successfully.", data: null));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                await _authService.LogoutAsync(email);
            }

            Response.Cookies.Delete("refresh_token");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<dynamic>.Failure("Unauthenticated"));

            var result = await _authService.GetActiveSessionsAsync(email);
            if (!result.status)
                return BadRequest(ApiResponse<dynamic>.Failure(result.message));

            return Ok(ApiResponse<dynamic>.Success(result.message, result.sessions));
        }

        [HttpPost("sessions/revoke-all")]
        public async Task<IActionResult> RevokeAllSessions()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<dynamic>.Failure("Unauthenticated"));

            var result = await _authService.RevokeAllSessionsAsync(email);
            if (!result.status)
                return BadRequest(ApiResponse<dynamic>.Failure(result.message));

            Response.Cookies.Delete("refresh_token");
            return Ok(new { message = result.message });
        }

        private void AppendRefreshTokenCookie(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var host = Request.Host.Host;
            var isLocalRequest =
                string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
            var useSecureCookie = Request.IsHttps || !isLocalRequest;

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookie,
                SameSite = useSecureCookie ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        private static string? GetStringProperty(object? value, string propertyName)
        {
            if (value == null)
                return null;

            return value.GetType().GetProperty(propertyName)?.GetValue(value) as string;
        }
    }


}
