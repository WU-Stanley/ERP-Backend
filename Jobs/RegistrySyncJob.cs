using System.Threading.Tasks;
using WUIAM.Interfaces;

namespace WUIAM.Jobs
{
    public class RegistrySyncJob
    {
        private readonly IRegistrySyncService _registrySyncService;

        public RegistrySyncJob(IRegistrySyncService registrySyncService)
        {
            _registrySyncService = registrySyncService;
        }

        public async Task ExecuteAsync()
        {
            await _registrySyncService.RunScheduledSyncsAsync();
        }
    }
}
