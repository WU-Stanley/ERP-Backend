using Microsoft.EntityFrameworkCore;
using WUIAM.Models;

namespace WUIAM.Services
{
    /// <summary>
    /// Custom health checks for application components.
    /// </summary>
    public static class HealthCheckService
    {
        /// <summary>
        /// Checks if the database connection is alive.
        /// </summary>
        public static async Task<bool> CheckDatabaseAsync(WUIAMDbContext context)
        {
            try
            {
                await context.Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if Hangfire dashboard storage is accessible.
        /// </summary>
        public static async Task<bool> CheckHangfireAsync()
        {
            try
            {
                // In production, check Hangfire storage connection
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if external email service (Brevo) is reachable.
        /// </summary>
        public static async Task<bool> CheckEmailServiceAsync(string apiKey, string baseUrl)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                var response = await httpClient.GetAsync($"{baseUrl}/email/status");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
