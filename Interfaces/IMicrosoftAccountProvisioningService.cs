using WUIAM.DTOs;

namespace WUIAM.Interfaces
{
    public interface IMicrosoftAccountProvisioningService
    {
        Task<MicrosoftAccountResult> CreateAccountAsync(string employeeName, string? jobTitle, CancellationToken cancellationToken = default);
    }
}
