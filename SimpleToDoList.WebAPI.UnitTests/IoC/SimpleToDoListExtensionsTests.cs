using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SimpleToDoList.Application.Interfaces;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.IoC;
using Xunit;

namespace SimpleToDoList.WebAPI.UnitTests.IoC
{
    public class SimpleToDoListExtensionsTests
    {
        [Fact]
        public void AddSimpleToDoList_ShouldAddServices_Where_AddExtension()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLogging();
            services.AddSimpleToDoList("");
            var provider = services.BuildServiceProvider();
            var database = provider.GetService<IDatabaseFileProvider>();
            var repository = provider.GetService<ISimpleTaskRepository>();
            var service = provider.GetService<ISimpleTaskService>();
            var notification = provider.GetService<INotificationContext>();

            // Assert
            database.Should().NotBeNull();
            repository.Should().NotBeNull();
            service.Should().NotBeNull();
            notification.Should().NotBeNull();
        }
    }
}
