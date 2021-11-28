using System.Collections.Generic;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Domain.Interfaces
{
    public interface INotificationContext
    {
        public void AddNotification(string message, string detail = "");
        public IEnumerable<NotificationMessage> Notifications { get; }
        public bool IsValid { get; }
    }
}
