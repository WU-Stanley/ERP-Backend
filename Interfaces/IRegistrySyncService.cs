using System;
using System.Threading.Tasks;

namespace WUIAM.Interfaces
{
    public interface IRegistrySyncService
    {
        Task SyncIntegrationAsync(Guid integrationId);
        Task RunScheduledSyncsAsync();
    }
}
