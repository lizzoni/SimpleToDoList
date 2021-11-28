using System.Collections.Generic;
using System.Linq;
using Flunt.Notifications;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Infrastructure.Notifications
{
    public class NotificationContext : Notifiable<Notification>, INotificationContext
    {
        public new void AddNotification(string message, string detail = "") => base.AddNotification(message, detail);

        public new IEnumerable<NotificationMessage> Notifications => base.Notifications.Select(notification => new NotificationMessage(notification.Key, notification.Message));
    }
}
