namespace WUIAM.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public object? data { get; set; }
        public string? token { get; set; }
        public string? refreshToken { get; set; }
        public bool verifyToken { get; set; }
        public bool emailSent { get; set; }
        public string Message { get; set; } = string.Empty;

        public static AuthResultDto Failure(string message)
            => new() { Success = false, Message = message };
    }

    public class AuthenticatedUserDto
    {
        public Guid id { get; set; }
        public string? userName { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public Guid userTypeId { get; set; }
        public bool isDefault { get; set; }
        public bool twoFactorEnabled { get; set; }
        public bool singleSignOnEnabled { get; set; }
        public DateTime? dateLastLoggedIn { get; set; }
        public string? sessionId { get; set; }
        public DateTime? sessionTime { get; set; }
        public List<AuthenticatedRoleDto> userRoles { get; set; } = new();
        public List<AuthenticatedPermissionDto> userPermissions { get; set; } = new();
    }

    public class AuthenticatedRoleDto
    {
        public Guid roleId { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<AuthenticatedPermissionDto>? rolePermissions { get; set; }
    }

    public class AuthenticatedPermissionDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
