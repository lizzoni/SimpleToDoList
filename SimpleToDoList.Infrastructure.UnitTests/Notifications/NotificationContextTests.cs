using System;
using System.Linq;
using FluentAssertions;
using SimpleToDoList.Infrastructure.Notifications;
using Xunit;

namespace SimpleToDoList.Infrastructure.UnitTests.Notifications
{
    public class NotificationContextTests
    {
        private readonly NotificationContext _notification;

        public NotificationContextTests()
        {
            _notification = new NotificationContext();
        }

        [Fact]
        public void AddNotification_ShouldAddNotification_WhenInputMessage()
        {
            // Arrange
            var message = Guid.NewGuid().ToString();
            var detail = Guid.NewGuid().ToString();

            // Act
            _notification.AddNotification(message, detail);

            // Assert
            _notification.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Notifications_ShouldHaveNotifications_WhenInputMessage()
        {
            // Arrange
            var message = Guid.NewGuid().ToString();
            var detail = Guid.NewGuid().ToString();

            // Act
            _notification.AddNotification(message, detail);

            // Assert
            _notification.Notifications.First().Message.Should().Be(message);
            _notification.Notifications.First().Detail.Should().Be(detail);
            _notification.Notifications.Should().HaveCount(1);
        }
    }
}
