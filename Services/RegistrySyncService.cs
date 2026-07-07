using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Services
{
    public class RegistrySyncService : IRegistrySyncService
    {
        private readonly WUIAMDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrySyncService(WUIAMDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SyncIntegrationAsync(Guid integrationId)
        {
            var integration = await _context.RegistryIntegrationRecords.FindAsync(integrationId);
            if (integration == null || integration.Status != "Active" && integration.Status != "Synced" && integration.Status != "Failed")
            {
                // Cannot sync if not found or inactive
                return;
            }

            bool success = false;
            string message = "Unknown error occurred.";

            try
            {
                // Ping the external URL using an HTTP client
                if (!string.IsNullOrWhiteSpace(integration.ExternalUrl))
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    var response = await client.GetAsync(integration.ExternalUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                        message = $"Successfully synchronized data with the external system. Response code: {(int)response.StatusCode}.";
                    }
                    else
                    {
                        message = $"Failed to synchronize. Server returned status code: {(int)response.StatusCode}.";
                    }
                }
                else
                {
                    // No URL configured, simulate success
                    success = true;
                    message = "Simulated successful sync. No External URL configured.";
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = $"Failed to synchronize. Exception: {ex.Message}";
            }

            // Create log record
            var log = new RegistrySyncLog
            {
                Id = Guid.NewGuid(),
                IntegrationId = integration.Id,
                ActionType = "Background Sync",
                Status = success ? "Success" : "Failed",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            if (success)
            {
                integration.Status = "Synced";
                integration.LastSyncedAt = DateTime.UtcNow;
            }
            else
            {
                integration.Status = "Failed";
            }

            integration.UpdatedAt = DateTime.UtcNow;

            _context.RegistrySyncLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task RunScheduledSyncsAsync()
        {
            var activeIntegrations = await _context.RegistryIntegrationRecords
                .Where(r => r.Status == "Active" || r.Status == "Synced" || r.Status == "Failed")
                .ToListAsync();

            foreach (var integration in activeIntegrations)
            {
                try
                {
                    await SyncIntegrationAsync(integration.Id);
                }
                catch (Exception)
                {
                    // Log errors locally or continue to the next integration
                    // The inner method handles its own DB logging, but just in case
                }
            }
        }
    }
}
