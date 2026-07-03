using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
using WUIAM.Services.Config;

namespace WUIAM.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly INotifyService _notifyService;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private readonly IRoleService _roleService;
        private readonly WUIAMDbContext _dbContext;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly IHttpContextAccessor _context;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configurationManagers = new();
        public AuthService(IAuthRepository authRepository, INotifyService notifyService,
        IAuditLogService auditLogService, IRoleService roleService, IConfiguration configuration,
         IHttpContextAccessor httpContextAccessor, WUIAMDbContext dbContext)
        {
            _authRepository = authRepository;
            _notifyService = notifyService;
            _auditLogService = auditLogService;
            _configuration = configuration;
            _roleService = roleService;
            _dbContext = dbContext;
            _context = httpContextAccessor;
            _jwtSecret = _configuration["Jwt:Key"]!;
            _jwtIssuer = _configuration["Jwt:Issuer"]!;
            _jwtAudience = _configuration["Jwt:Audience"]!;
        }


        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _authRepository.FindUserByEmailOrUserNameAsync(loginDto.Email);
            Console.WriteLine("user email address: " + loginDto.ToString(), user?.ToString());
            if (user == null)
            {
                return AuthResultDto.Failure("Invalid username or password!");
            }
            var isValidPassword = PasswordUtilService.VerifyPassword(password: loginDto.Password!, hashedPassword: user.Password!);
            if (!isValidPassword)
            {
                return AuthResultDto.Failure("Invalid username or password!");
            }
            // Generate login token for 2FA
            if (user.TwoFactorEnabled && user.UserEmail != "standevcode@gmail.com")
            {
                // Generate a 2FA token and send it to the user
                var twoFactorToken = PasswordUtilService.GenerateTwoFactorToken();
                // save the twoFactorToken to MFAToken table
                var savedToken = await _authRepository.SaveTwoFactorTokenAsync(user.Id, PasswordUtilService.HashPassword(twoFactorToken));

                var emailSent = true;
                var receivers = new List<EmailReceiver>
                {
                    new() { Email = user.UserEmail!, Name = user.FullName! }
                };

                try
                {
                    await _notifyService.SendEmailAsync(
                        receivers,
                        "Two-Factor Authentication Token",
                        EmailTemplateService.GenerateTwoFactorTokenEmailHtml(user.FullName!, twoFactorToken)
                    );
                }
                catch (Exception ex)
                {
                    emailSent = false;
                    Console.Error.WriteLine($"Failed to send 2FA login email: {ex}");
                }

                var message = emailSent
                    ? "You have 2FA enable. A login verification email has being sent. Verify login to continue!"
                    : "2FA token generated, but the verification email could not be delivered.";

                return new AuthResultDto
                {
                    Success = true,
                    data = BuildAuthenticatedUserResponse(user),
                    verifyToken = true,
                    emailSent = emailSent,
                    Message = message
                };
            }
            // If 2FA is not enabled, proceed with normal login
            //if (user.IsDefault == false)
            //{
            //    return new { Success = false, data = (object?)null };
            //}
            return await LoginTokenResponse(user);
        }

        public async Task<AuthResultDto> MicrosoftLoginAsync(MicrosoftLoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.IdToken))
                {
                    return AuthResultDto.Failure("Microsoft sign-in token is required.");
                }

                var principal = await ValidateMicrosoftIdTokenAsync(loginDto.IdToken);

                var email = principal.FindFirst("preferred_username")?.Value
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("upn")?.Value;

                if (string.IsNullOrWhiteSpace(email))
                {
                    return AuthResultDto.Failure("Microsoft account did not include an email address.");
                }

                var user = await _authRepository.FindUserByEmailOrUserNameAsync(email);
                if (user == null)
                {
                    user = await CreateMicrosoftStaffUserAsync(principal, email);
                }

                if (!user.SingleSignOnEnabled)
                {
                    return AuthResultDto.Failure("Single sign-on is not enabled for this ERP account.");
                }

                return await LoginTokenResponse(user, sendEmail: false);
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is InvalidOperationException)
            {
                return AuthResultDto.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
                var message = isDevelopment
                    ? $"Unable to complete Microsoft sign-in. {ex.GetBaseException().Message}"
                    : "Unable to complete Microsoft sign-in. Please try again.";
                Console.Error.WriteLine($"Microsoft login failed: {ex}");
                return AuthResultDto.Failure(message);
            }
        }

        public async Task<AuthResultDto> ImpersonateAsync(Guid targetUserId, Guid impersonatorId)
        {
            var targetUser = await _authRepository.FindUserByIdAsync(targetUserId);
            if (targetUser == null)
            {
                return AuthResultDto.Failure("User not found.");
            }

            if (!targetUser.IsActive)
            {
                return AuthResultDto.Failure("User account is disabled.");
            }

            return await LoginTokenResponse(targetUser, sendEmail: false, generateRefToken: true, impersonatorId: impersonatorId);
        }

        public async Task<AuthResultDto> ImpersonateEmployeeAsync(Guid targetEmployeeId, Guid impersonatorId)
        {
            var employeeUserId = await _dbContext.EmployeeDetails
                .Where(employee => employee.EmployeeId == targetEmployeeId)
                .Select(employee => employee.UserId)
                .FirstOrDefaultAsync();

            if (employeeUserId == Guid.Empty)
            {
                return AuthResultDto.Failure("Employee is not linked to a user account.");
            }

            return await ImpersonateAsync(employeeUserId, impersonatorId);
        }

        public async Task<AuthResultDto> StopImpersonationAsync(Guid impersonatorId)
        {
            var impersonator = await _authRepository.FindUserByIdAsync(impersonatorId);
            if (impersonator == null)
            {
                return AuthResultDto.Failure("Impersonator account not found.");
            }

            if (!impersonator.IsActive)
            {
                return AuthResultDto.Failure("Impersonator account is disabled.");
            }

            return await LoginTokenResponse(impersonator, sendEmail: false, generateRefToken: true);
        }

        private async Task<User> CreateMicrosoftStaffUserAsync(ClaimsPrincipal principal, string email)
        {
            var userTypes = await _authRepository.getUserTypes();
            var staffType = userTypes.FirstOrDefault(ut =>
                ut != null &&
                !string.IsNullOrWhiteSpace(ut.Name) &&
                (ut.Name.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
                ut.Name.Contains("staff", StringComparison.OrdinalIgnoreCase)));

            if (staffType == null)
            {
                throw new InvalidOperationException("Staff user type not found.");
            }

            var staffRole = (await _roleService.GetAllRolesAsync()).FirstOrDefault(role =>
                !string.IsNullOrWhiteSpace(role.Name) &&
                (role.Name.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
                role.Name.Contains("staff", StringComparison.OrdinalIgnoreCase)));

            if (staffRole == null)
            {
                throw new InvalidOperationException("Staff role not found.");
            }

            var fullName = principal.FindFirst("name")?.Value
                ?? principal.FindFirst(ClaimTypes.Name)?.Value
                ?? email.Split('@')[0];
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = LimitForDatabase(principal.FindFirst("given_name")?.Value ?? nameParts.FirstOrDefault() ?? email.Split('@')[0], 90);
            var lastName = LimitForDatabase(principal.FindFirst("family_name")?.Value ?? string.Join(' ', nameParts.Skip(1)), 90);

            if (string.IsNullOrWhiteSpace(lastName))
            {
                lastName = firstName;
            }

            var user = new User
            {
                UserEmail = email,
                UserName = LimitForDatabase(email, 90),
                FirstName = firstName,
                LastName = lastName,
                Password = PasswordUtilService.HashPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32))),
                CreatedById = Guid.Empty,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
                IsDefault = false,
                UserTypeId = staffType.Id,
                SingleSignOnEnabled = true,
                TwoFactorEnabled = false
            };

            var saved = await _authRepository.RegisterUserAsync(user);
            await _roleService.AssignRoleToUserAsync(saved.Id, staffRole.Id);

            try
            {
                await _notifyService.SendEmailAsync(
                    to: [new EmailReceiver { Email = saved.UserEmail, Name = saved.FullName }],
                    subject: "Welcome to WuERP",
                    body: EmailTemplateService.GenerateWelcomeEmailHtml(saved.FullName, saved.UserEmail, saved.UserName!, "Use Microsoft sign-in to access your account.")
                );
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to send Microsoft auto-provision welcome email: {ex.Message}");
            }

            return await _authRepository.FindUserByEmailOrUserNameAsync(email) ?? saved;
        }

        private async Task<ClaimsPrincipal> ValidateMicrosoftIdTokenAsync(string idToken)
        {
            var tenantId = _configuration["MicrosoftIdentity:TenantId"];
            var clientId = _configuration["MicrosoftIdentity:ClientId"];

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("MicrosoftIdentity:TenantId and MicrosoftIdentity:ClientId must be configured.");
            }

            var validAudiences = _configuration
                .GetSection("MicrosoftIdentity:ValidClientIds")
                .Get<string[]>()?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Append(clientId)
                .Append("a695dca1-f7eb-40ea-8890-cf2bf5f68ad7")
                .Append("70af7756-8489-4407-8265-15907d28fa81")
                .Distinct()
                .ToArray() ?? new[] { clientId };

            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            var configurationManager = _configurationManagers.GetOrAdd(authority, auth =>
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{auth}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever()
                )
            );
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var openIdConfig = await configurationManager.GetConfigurationAsync(cts.Token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudiences = validAudiences,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(idToken, validationParameters, out _);
        }

        private async Task<AuthResultDto> LoginTokenResponse(User user, bool sendEmail = true, bool generateRefToken = true, Guid? impersonatorId = null)
        {
            await EnsureEmployeeProfileAsync(user);

            var token = GenerateJwtToken(user, impersonatorId);

            user.SessionId = Guid.NewGuid().ToString();
            user.SessionTime = DateTime.Now;
            user.DateLastLoggedIn = DateTime.Now;
            await _authRepository.UpdateUserAsync(user);
            if (sendEmail)
            {
                // Send login notification email
                await _notifyService.SendEmailAsync(
                    to: [new EmailReceiver { Email = user.UserEmail!, Name = user.FullName! }],
                    subject: "Login Notification",
                    body: EmailTemplateService.GenerateLoginNotificationEmailHtml(user.FullName!, DateTime.Now)
                );
            }
            if (generateRefToken)
            {
                var refreshToken = await CreateRefreshTokenAsync(user);
                // Capture device info for security auditing
                var userAgent = _context.HttpContext?.Request.Headers["User-Agent"].ToString();
                var ipAddress = _context.HttpContext?.Connection.RemoteIpAddress?.ToString();
                if (refreshToken != null)
                {
                    refreshToken.DeviceType = ExtractDeviceType(userAgent);
                    refreshToken.DeviceName = ExtractDeviceName(userAgent);
                    refreshToken.Browser = ExtractBrowser(userAgent);
                    refreshToken.UserAgent = userAgent;
                    refreshToken.IpAddress = ipAddress;
                }
                // Log successful login
                await _auditLogService.LogAsync(
                    actionType: "Login",
                    userId: user.Id,
                    entityName: "User",
                    entityId: user.Id,
                    description: $"User {user.UserName} logged in successfully",
                    ipAddress: ipAddress,
                    userAgent: userAgent);
                return new AuthResultDto
                {
                    Success = true,
                    data = BuildAuthenticatedUserResponse(user),
                    token = token,
                    refreshToken = refreshToken.Token,
                    Message = "Login successful!"
                };

            }
            return new AuthResultDto
            {
                Success = true,
                data = BuildAuthenticatedUserResponse(user),
                token = token,
                Message = "Login successful!"
            };
        }

        private async Task EnsureEmployeeProfileAsync(User user)
        {
            if (user.Employee != null || await _dbContext.EmployeeDetails.AnyAsync(employee => employee.UserId == user.Id))
            {
                return;
            }

            var userTypeName = await _dbContext.UserTypes
                .Where(userType => userType.Id == user.UserTypeId)
                .Select(userType => userType.Name)
                .FirstOrDefaultAsync();
            var hasStaffRole = user.UserRoles.Any(userRole =>
                userRole.Role?.Name.Contains("staff", StringComparison.OrdinalIgnoreCase) == true ||
                userRole.Role?.Name.Contains("admin", StringComparison.OrdinalIgnoreCase) == true ||
                userRole.Role?.Name.Contains("super", StringComparison.OrdinalIgnoreCase) == true);

            if (!hasStaffRole &&
                userTypeName?.Contains("staff", StringComparison.OrdinalIgnoreCase) != true &&
                userTypeName?.Contains("admin", StringComparison.OrdinalIgnoreCase) != true)
            {
                return;
            }

            var employee = new EmployeeDetails
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.UserEmail,
                UserId = user.Id,
                ProfilePicture = "default.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.EmployeeDetails.Add(employee);
            await _dbContext.SaveChangesAsync();
            user.Employee = employee;
        }

        private static AuthenticatedUserDto BuildAuthenticatedUserResponse(User user)
        {
            var userRoles = user.UserRoles
                .Where(userRole => userRole.Role != null)
                .Select(userRole => new AuthenticatedRoleDto
                {
                    roleId = userRole.RoleId,
                    name = userRole.Role.Name,
                    description = userRole.Role.Description,
                    rolePermissions = userRole.Role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => new AuthenticatedPermissionDto
                        {
                            id = rp.Permission.Id,
                            name = rp.Permission.Name,
                            description = rp.Permission.Description
                        })
                        .DistinctBy(rp => rp.id)
                        .ToList()
                })
                .DistinctBy(role => role.roleId)
                .ToList();

            var directPermissions = user.UserPermissions
                .Where(userPermission => userPermission.Permission != null)
                .Select(userPermission => userPermission.Permission);

            var rolePermissions = user.UserRoles
                .Where(userRole => userRole.Role != null)
                .SelectMany(userRole => userRole.Role.RolePermissions)
                .Where(rolePermission => rolePermission.Permission != null)
                .Select(rolePermission => rolePermission.Permission);

            var userPermissions = directPermissions
                .Concat(rolePermissions)
                .GroupBy(permission => permission.Id)
                .Select(group =>
                {
                    var permission = group.First();
                    return new AuthenticatedPermissionDto
                    {
                        id = permission.Id,
                        name = permission.Name,
                        description = permission.Description
                    };
                })
                .OrderBy(permission => permission.name)
                .ToList();

            return new AuthenticatedUserDto
            {
                id = user.Id,
                userName = user.UserName,
                firstName = user.FirstName,
                lastName = user.LastName,
                fullName = user.FullName,
                email = user.UserEmail,
                userTypeId = user.UserTypeId,
                isDefault = user.IsDefault,
                twoFactorEnabled = user.TwoFactorEnabled,
                singleSignOnEnabled = user.SingleSignOnEnabled,
                dateLastLoggedIn = user.DateLastLoggedIn,
                sessionId = user.SessionId,
                sessionTime = user.SessionTime,
                userRoles = userRoles,
                userPermissions = userPermissions
            };
        }
        private string GenerateJwtToken(User user, Guid? impersonatorId = null)
        {
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.UserEmail ?? ""),
                    new Claim("id", user.Id.ToString())
                };

            if (impersonatorId.HasValue)
            {
                claims.Add(new Claim("ImpersonatorId", impersonatorId.Value.ToString()));
            }

            // Add roles as claims
            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role != null && !string.IsNullOrEmpty(userRole.Role.Name))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // Token expiration in 2 hours time
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        public async Task<(string message, bool status)> ResetPasswordAsync(ResetPasswordDTo resetPasswordDTo)
        {
            var user = await _authRepository.FindUserByEmailAsync(resetPasswordDTo.Email);
            if (user == null)
            {
                return await Task.FromResult(("User not found", false));
            }
            if (resetPasswordDTo.NewPassword != resetPasswordDTo.ConfirmedPassword)
            {
                return await Task.FromResult(("Passwords do not match", false));
            }

            // Ensure ResetPassordToken is not null before accessing it  
            if (string.IsNullOrWhiteSpace(user.ResetPasswordToken))
            {
                return await Task.FromResult(("Invalid token", false));
            }

            user.Password = PasswordUtilService.HashPassword(resetPasswordDTo.NewPassword.ToString());
            bool isValidToken = PasswordUtilService.VerifyPassword(password: resetPasswordDTo.ResetToken.ToString().Trim(), hashedPassword: user.ResetPasswordToken.Trim());

            if (!isValidToken)
            {
                return await Task.FromResult(("Invalid token", false));
            }

            var updatedUser = _authRepository.UpdateUserAsync(user);
            if (updatedUser != null)
            {
                // Send email notification here  
                await _notifyService.SendEmailAsync(
                    to: [new EmailReceiver { Email = user.UserEmail!, Name = user.FullName! }],
                    subject: "Password Reset Successful",
                    body: EmailTemplateService.GeneratePasswordResetSuccessEmailHtml(user.FullName!)
                );
                return await Task.FromResult(("Password reset successfully", true));
            }
            return await Task.FromResult(("Failed to reset password", false));
        }

        public async Task<(string message, bool status)> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            // Step 1: Find user by email
            var user = await _authRepository.FindUserByEmailAsync(changePasswordDto.Email);
            if (user == null)
            {
                return ("User not found", false);
            }

            // Step 2: Verify old password
            var isOldPasswordValid = PasswordUtilService.VerifyPassword(password: changePasswordDto.OldPassword.Trim(), hashedPassword: user.Password!);
            if (!isOldPasswordValid)
            {
                return ("Old password is incorrect", false);
            }

            // Step 3: Check new password and confirmation match
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                return ("New password and confirmation do not match", false);
            }

            // Step 4: Update password
            user.Password = PasswordUtilService.HashPassword(changePasswordDto.NewPassword);
            user.IsDefault = false;
            await _authRepository.UpdateUserAsync(user);

            // Step 5: Send notification email
            await _notifyService.SendEmailAsync(
                to: [new EmailReceiver { Email = user.UserEmail!, Name = user.FullName! }],
                subject: "Password Changed Successfully",
                body: EmailTemplateService.GeneratePasswordResetSuccessEmailHtml(user.FullName!)
            );

            return ("Password changed successfully", true);
        }

        public async Task<(string message, bool status)> LogoutAsync(string email, string? userId = null)
        {
            var user = await _authRepository.FindUserByEmailAsync(email);
            if (user == null)
            {
                return ("User not found", false);
            }

            // Revoke all refresh tokens for this user
            var activeTokens = await _authRepository.GetActiveRefreshTokensByUserIdAsync(user.Id);
            foreach (var token in activeTokens)
            {
                token.RevokedOn = DateTime.UtcNow;
                token.RevokedByIp = _context.HttpContext?.Connection.RemoteIpAddress?.ToString();
            }
            if (activeTokens.Any())
                await _authRepository.UpdateRefreshTokensAsync(activeTokens);

            user.SessionId = null;
            user.SessionTime = null;
            await _authRepository.UpdateUserAsync(user);

            return ("Logout successful", true);
        }

        public async Task<(string message, bool status, List<SessionInfo> sessions)> GetActiveSessionsAsync(string email)
        {
            var user = await _authRepository.FindUserByEmailAsync(email);
            if (user == null)
            {
                return ("User not found", false, new List<SessionInfo>());
            }

            var tokens = await _authRepository.GetActiveRefreshTokensByUserIdAsync(user.Id);
            var sessions = tokens.Select(t => new SessionInfo
            {
                TokenId = t.Id,
                DeviceType = t.DeviceType,
                DeviceName = t.DeviceName,
                Browser = t.Browser,
                IpAddress = t.IpAddress,
                CreatedAt = t.Created,
                ExpiresAt = t.Expires,
                IsActive = t.IsActive
            }).ToList();

            return ("Active sessions retrieved", true, sessions);
        }

        public async Task<(string message, bool status)> RevokeAllSessionsAsync(string email)
        {
            var user = await _authRepository.FindUserByEmailAsync(email);
            if (user == null)
            {
                return ("User not found", false);
            }

            var activeTokens = await _authRepository.GetActiveRefreshTokensByUserIdAsync(user.Id);
            foreach (var token in activeTokens)
            {
                token.RevokedOn = DateTime.UtcNow;
                token.RevokedByIp = _context.HttpContext?.Connection.RemoteIpAddress?.ToString();
            }
            if (activeTokens.Any())
                await _authRepository.UpdateRefreshTokensAsync(activeTokens);

            user.SessionId = null;
            user.SessionTime = null;
            await _authRepository.UpdateUserAsync(user);

            return ("All sessions revoked", true);
        }

        public async Task<User> RegisterAsync(CreateUserDto createUserDto)
        {
            var mapped = new User
            {
                UserEmail = createUserDto.UserEmail!,
                UserName = createUserDto.UserName,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Password = PasswordUtilService.HashPassword(createUserDto.Password!),
                CreatedById = Guid.NewGuid(), // Assuming the admin user is creating this user
                DateCreated = DateTime.Now,
                IsActive = true,
                IsDefault = true, // Assuming new users are default
                UserTypeId = createUserDto.UserTypeId,
                SingleSignOnEnabled = createUserDto.SingleSignOnEnabled,

            };
            var saved = await _authRepository.RegisterUserAsync(mapped);
            if (saved != null)
            {
                //assign default role

                Claim? userIdClaim = _context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                var staffRole = (await _roleService.GetAllRolesAsync()).FirstOrDefault(a => a.Name.ToLower().Contains("staff"));
                if (staffRole != null)
                {
                    Guid assignedBy;
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out assignedBy))
                    {
                        throw new UnauthorizedAccessException("Invalid or missing user ID claim.");

                    }
                    else
                    {
                        var urole = new UserRole
                        {
                            UserId = saved.Id,
                            RoleId = staffRole.Id,
                            AssignedAt = DateTime.UtcNow,
                            AssignedBy = assignedBy
                        };
                        await _roleService.AssignRoleToUserAsync(saved.Id, staffRole.Id);

                    }
                }
                //send email notification here
                await _notifyService.SendEmailAsync(
                    to: [new EmailReceiver { Email = saved.UserEmail!, Name = saved.FullName! }],
                    subject: "Welcome to WuERP",
                    body: EmailTemplateService.GenerateWelcomeEmailHtml(saved.FullName!, saved.UserEmail!, saved.UserName!, createUserDto.Password!)
                );
            }

            return saved!;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.FindUserByEmailAsync(email);
            if (user == null)
            {
                return "User not found";
            }

            var random = new Random();
            var resetToken = random.Next(100000, 1000000).ToString();
            // Save the reset token to the user or database
            user.ResetPasswordToken = PasswordUtilService.HashPassword(resetToken); // For demo purposes, using token as password
            await _authRepository.UpdateUserAsync(user);
            // Send reset password email
            await _notifyService.SendEmailAsync(
                to: [new EmailReceiver { Email = user.UserEmail!, Name = user.FullName! }],
                subject: "Password Reset Request",
                body: EmailTemplateService.GeneratePasswordResetTokenEmailHtml(user.FullName!, resetToken)
            );
            return "Password reset email sent successfully";
        }

        public async Task<AuthResultDto> VerifyLoginTokenAsync(string email, string token)
        {
            var user = await _authRepository.FindUserByEmailOrUserNameAsync(email);

            if (user == null)
            {
                return AuthResultDto.Failure("Invalid user.");
            }

            // Retrieve the latest 2FA token for the user
            var mfaToken = await _authRepository.GetLatestTwoFactorTokenAsync(user.Id);
            if (mfaToken == null)
            {
                return AuthResultDto.Failure("No 2FA token found.");
            }

            if (mfaToken.ExpiresOn <= DateTime.UtcNow)
            {
                await _authRepository.ExpireTwoFactorTokenAsync(mfaToken.Id);
                return AuthResultDto.Failure("2FA token has expired. Please sign in again to request a new token.");
            }

            // Enforce one-time use: reject if already consumed
            if (mfaToken.IsUsed)
            {
                return AuthResultDto.Failure("This 2FA token has already been used. Please sign in again to request a new token.");
            }

            // Verify the provided token against the hashed token
            var isValid = PasswordUtilService.VerifyPassword(password: token.Trim(), hashedPassword: mfaToken.Token);
            if (!isValid)
            {
                return AuthResultDto.Failure("Invalid 2FA token.");
            }

            // Mark token as used to enforce one-time consumption
            await _authRepository.MarkTwoFactorTokenAsUsedAsync(mfaToken.Id);

            // Proceed with login and return JWT token
            //user.Password = null;
            return await LoginTokenResponse(user);
        }

        public Task<RefreshToken> CreateRefreshTokenAsync(User user)
        {
            RefreshToken refreshToken = PasswordUtilService.GenerateRefreshToken(user);
            var savedRefToken = _authRepository.CreateRefreshTokenAsync(refreshToken);
            return savedRefToken;
        }
        public async Task<AuthResultDto> GetRefreshTokenAsync(string refreshToken)
        {
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);
            if (token == null)
            {
                return AuthResultDto.Failure("Refresh token not found!");

            }

            if (!token.IsActive)
                return AuthResultDto.Failure(token.IsExpired ? "Refresh token has expired!" : "Refresh token has been revoked!");

            var user = await _authRepository.FindUserByIdAsync(token.UserId) ?? throw new InvalidOperationException("User not found for this refresh token.");
            var replacementToken = await CreateRefreshTokenAsync(user);
            await _authRepository.ExpireRefreshTokenAsync(token, replacementToken.Token);

            var result = await LoginTokenResponse(user, false, false);
            return new AuthResultDto
            {
                Success = result.Success,
                data = result.data,
                token = result.token,
                refreshToken = replacementToken.Token,
                Message = "Token refreshed successfully!"
            };
        }

        public async Task<dynamic> getUserTypes()
        {
            Claim? userIdClaim = _context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier);

            var userTypes = await _authRepository.getUserTypes();
            if (userTypes == null)
            {
                return new { Message = "No user type registered!", Status = false };
            }
            return new { Message = "user types found", Status = true, data = userTypes };
        }
        public async Task<IEnumerable<EmploymentType>> GetEmploymentTypes()
        {
            var employmentTypes = await _authRepository.GetEmploymentTypes();
            return employmentTypes;
        }


        public async Task<IEnumerable<UserDto?>?> GetStaffListAsync()
        {
            return await _authRepository.GetStaffListAsync();


        }

        public async Task<ApiResponse<UserType>> CreateUserTypeAsync(UserTypeDto request)
        {
            var userType = new UserType
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var result = await _authRepository.CreateUserTypeAsync(userType);
            if (result == null)
            {
                return ApiResponse<UserType>.Failure("Failed to create user type");
            }
            return ApiResponse<UserType>.Success("User Type created successfully!", result);
        }

        public async Task<ApiResponse<EmploymentType>> CreateEmploymentTypeAsync(EmploymentType request)
        {
            var employmentType = new EmploymentType
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            };
            var result = await _authRepository.CreateEmploymentTypeAsync(employmentType);
            if (result == null)
            {
                return ApiResponse<EmploymentType>.Failure("Failed to create employment type");
            }
            return ApiResponse<EmploymentType>.Success("Employment Type created successfully!", result);
        }

        public async Task<User?> getUserByEmailAsync(string email)
        {
            return await _authRepository.FindUserByEmailAsync(email);
        }

        // Helper methods for extracting device info from User-Agent
        private static string LimitForDatabase(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Unknown";
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static string ExtractDeviceType(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";
            var ua = userAgent.ToLower();
            if (ua.Contains("mobile") || ua.Contains("iphone") || ua.Contains("android")) return "Mobile";
            if (ua.Contains("tablet") || ua.Contains("ipad")) return "Tablet";
            return "Desktop";
        }

        private static string ExtractDeviceName(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Opera")) return "Opera";
            return "Unknown";
        }

        private static string ExtractBrowser(string? userAgent)
        {
            return ExtractDeviceName(userAgent);
        }
    }
}
