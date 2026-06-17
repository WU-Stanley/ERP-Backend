using WUIAM.Interfaces;
using WUIAM.Models;
using brevo_csharp.Api; 
using brevo_csharp.Model;
using Task = System.Threading.Tasks.Task;
using WUIAM.DTOs;

namespace WUIAM.Services
{
    public class NotifyService : INotifyService
    {
        private readonly ILogger<NotifyService> _logger;
        private readonly INotificationService _notificationService;

        public NotifyService(ILogger<NotifyService> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
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


        public Task SendSmsAsync(string to, string message)
        {
            _logger.LogInformation("SMS notification queued for {Recipient}: {Message}", to, message);
            return Task.CompletedTask;
        }

        public async Task NotifyRecruitmentTeamAsync(string title, string message, string type = "info", string? entityType = null, Guid? entityId = null)
        {
            await _notificationService.NotifyRecruitmentTeamAsync(title, message, type, entityType, entityId);
            _logger.LogInformation("Recruitment team notified: {Title} - {Message}", title, message);
        }
    }
}
