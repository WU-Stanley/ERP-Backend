using WUIAM.Interfaces;
using brevo_csharp.Api; 
using brevo_csharp.Model;
using Task = System.Threading.Tasks.Task;
using WUIAM.DTOs;

namespace WUIAM.Services
{
    public class NotifyService : INotifyService
    {
        public Task LogNotificationAsync(string userId, string message)
        {
            throw new NotImplementedException();
        }

        public Task PushNotificationAsync(string to, string title, string message)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
