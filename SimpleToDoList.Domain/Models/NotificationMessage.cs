namespace SimpleToDoList.Domain.Models
{
    public class NotificationMessage
    {
        public NotificationMessage(string message, string detail)
        {
            Message = message;
            Detail = detail;
        }

        public string Message { get; }
        public string Detail { get; }
    }
}
