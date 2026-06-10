﻿using WUIAM.DTOs;

namespace WUIAM.Interfaces
{
    public interface INotifyService
    {
        // Define methods for the notification service
        Task SendEmailAsync(List<EmailReceiver> to, string subject, string body);
        Task SendSmsAsync(string to, string message);
        Task PushNotificationAsync(string to, string title, string message);
        Task LogNotificationAsync(string userId, string message);
    }
}