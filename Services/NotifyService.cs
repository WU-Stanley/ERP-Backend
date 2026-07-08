using WUIAM.Interfaces;
using WUIAM.Models;
using brevo_csharp.Api; 
using brevo_csharp.Model;
using Task = System.Threading.Tasks.Task;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WUIAM.DTOs;
namespace WUIAM.Services
{
    public class NotifyService : INotifyService
    {
        private readonly ILogger<NotifyService> _logger;
        private readonly INotificationService _notificationService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NotifyService(
            ILogger<NotifyService> logger, 
            INotificationService notificationService,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _notificationService = notificationService;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public Task LogNotificationAsync(string userId, string message)
        {
            _logger.LogInformation("Notification for user {UserId}: {Message}", userId, message);
            return Task.CompletedTask;
        }

        public Task PushNotificationAsync(string to, string title, string message)
        {
            _logger.LogInformation("Push notification queued for {Recipient}: {Title} - {Message}", to, title, message);
            return Task.CompletedTask;
        }

        public Task SendEmailAsync(List<EmailReceiver> receivers, string subject, string body)
        {
            // Initialize Brevo client
            var apiInstance = new TransactionalEmailsApi();
            
            var emailMessage = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender("Team IT - Wigwe University","teamit@wigweuniversity.edu.ng"),
                To = receivers.Select(emailReceiver => new SendSmtpEmailTo(emailReceiver.Email,emailReceiver.Name)).ToList(),
                Subject = subject,
                HtmlContent = body
            };
            return apiInstance.SendTransacEmailAsync(emailMessage);
        }


        public async Task SendSmsAsync(string to, string message)
        {
            try
            {
                var apiToken = _configuration["BulkSmsNigeria:ApiToken"];
                var senderId = _configuration["BulkSmsNigeria:SenderId"] ?? "Wigwe Uni";

                if (string.IsNullOrEmpty(apiToken))
                {
                    _logger.LogWarning("BulkSmsNigeria API Token is missing. Mock SMS for {Recipient}: {Message}", to, message);
                    return;
                }

                var url = "https://www.bulksmsnigeria.com/api/v2/sms";
                
                var payload = new
                {
                    from = senderId,
                    to = to,
                    body = message,
                    dnd = 2
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to {Recipient}: {Response}", to, responseContent);
                }
                else
                {
                    _logger.LogError("Failed to send SMS to {Recipient}. Status: {StatusCode}, Error: {Error}", to, response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending SMS to {Recipient}", to);
            }
        }

        public async Task NotifyRecruitmentTeamAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null)
        {
            await _notificationService.NotifyRecruitmentTeamAsync(title, message, type, entityType, entityId);
            _logger.LogInformation("Recruitment team notified: {Title} - {Message}", title, message);
        }
    }
}
