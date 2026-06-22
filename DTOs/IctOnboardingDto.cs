namespace WUIAM.DTOs
{
    public class IctOnboardingDto
    {
        public Guid ApplicationId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string PersonalEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? MicrosoftEmail { get; set; }
        public string? MicrosoftUserId { get; set; }
        public DateTime? ProvisionedAt { get; set; }
        public string? Error { get; set; }
    }

    public class MicrosoftAccountProvisioningDto : IctOnboardingDto
    {
        public string? TemporaryPassword { get; set; }
    }

    public record MicrosoftAccountResult(string UserId, string UserPrincipalName, string TemporaryPassword);
}
