using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SimpleToDoList.Application.DTOs;
using SimpleToDoList.Application.Services;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;
using Xunit;

namespace SimpleToDoList.Application.UnitTests.Services
{
    public class SimpleTaskServiceTests
    {
        private readonly ISimpleTaskRepository _simpleTaskRepository;
        private readonly INotificationContext _notification;
        private readonly ILogger<SimpleTaskService> _logger;

        private readonly SimpleTaskService _simpleTaskService;

        public SimpleTaskServiceTests()
        {
            _simpleTaskRepository = Substitute.For<ISimpleTaskRepository>();
            _notification = Substitute.For<INotificationContext>();
            _logger = Substitute.For<ILogger<SimpleTaskService>>();

            _simpleTaskService = new SimpleTaskService(_simpleTaskRepository, _notification, _logger);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTask_WhenSimpleTaskDTOIsValid()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();

            _notification.IsValid.Returns(true);

            // Act
            var result = await _simpleTaskService.CreateAsync(simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).CreateAsync(Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Description.Should().Be(simpleTaskDTO.Description);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotCreateTask_WhenSimpleTaskDTOIsInvalid()
        {
            // Arrange
            var simpleTaskDTO = new SimpleTaskDTO();

            _notification.IsValid.Returns(false);

            // Act
            var result = await _simpleTaskService.CreateAsync(simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(0).CreateAsync(Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.Received(2).AddNotification("Missing Mandatory Parameter", Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldNotCreateTask_WhenPriorityParameterValueIsInvalid()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            simpleTaskDTO.Priority = (Priority)15;

            _notification.IsValid.Returns(false);

            // Act
            var result = await _simpleTaskService.CreateAsync(simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(0).CreateAsync(Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.Received(1).AddNotification("Invalid Parameter Value", Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_ShouldGetTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(id);

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .Returns(simpleTask);

            // Act
            var result = await _simpleTaskService.GetAsync(id, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ShouldNotGetTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _notification.IsValid.Returns(false);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .ReturnsNull();

            // Act
            var result = await _simpleTaskService.GetAsync(id, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            _notification.Received(1).AddNotification("Task not found", Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldGetAllTasks_WhenTasksExist()
        {
            // Arrange
            var query = new SimpleTaskQuery();
            var simpleTask1 = CreateSimpleTask(Guid.NewGuid());
            var simpleTask2 = CreateSimpleTask(Guid.NewGuid());

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask1, simpleTask2 });

            // Act
            var result = await _simpleTaskService.GetAllAsync(query, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnNothing_WhenTasksDontExist()
        {
            // Arrange
            var query = new SimpleTaskQuery();
            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask>());

            // Act
            var result = await _simpleTaskService.GetAllAsync(query, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();
            var simpleTask = CreateSimpleTask(id);

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .Returns(simpleTask);

            // Act
            var result = await _simpleTaskService.UpdateAsync(id, simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            await _simpleTaskRepository.Received(1).UpdateAsync(id, Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Description.Should().Be(simpleTaskDTO.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotUpdateTask_WhenInvalidParameters()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = new SimpleTaskUpdateDTO();

            _notification.IsValid.Returns(false);

            // Act
            var result = await _simpleTaskService.UpdateAsync(id, simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.DidNotReceive().GetAsync(id, Arg.Any<CancellationToken>());
            await _simpleTaskRepository.DidNotReceive().UpdateAsync(id, Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.Received(2).AddNotification("Missing Mandatory Parameter", Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotUpdateTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .ReturnsNull();

            // Act
            var result = await _simpleTaskService.UpdateAsync(id, simpleTaskDTO, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            await _simpleTaskRepository.DidNotReceive().UpdateAsync(id, Arg.Is<SimpleTask>(x => x.Description.Equals(simpleTaskDTO.Description)), Arg.Any<CancellationToken>());
            _notification.Received(1).AddNotification("Task not found", Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(id);

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .Returns(simpleTask);

            // Act
            var result = await _simpleTaskService.DeleteAsync(id, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            await _simpleTaskRepository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
            _notification.DidNotReceive().AddNotification(Arg.Any<string>(), Arg.Any<string>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotDeleteTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _notification.IsValid.Returns(true);
            _simpleTaskRepository.GetAsync(id, Arg.Any<CancellationToken>())
                .ReturnsNull();

            // Act
            var result = await _simpleTaskService.DeleteAsync(id, new CancellationToken());

            // Assert
            await _simpleTaskRepository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
            await _simpleTaskRepository.DidNotReceive().DeleteAsync(id, Arg.Any<CancellationToken>());
            _notification.Received(1).AddNotification("Task not found", Arg.Any<string>());
            result.Should().BeNull();
        }

        private static SimpleTaskDTO CreateSimpleTaskDTO(Priority priority = Priority.Medium)
        {
            return new SimpleTaskDTO
            {
                Description = Guid.NewGuid().ToString(),
                Priority = priority,
            };
        }

        private static SimpleTaskUpdateDTO CreateSimpleTaskUpdateDTO()
        {
            return new SimpleTaskUpdateDTO
            {
                Description = Guid.NewGuid().ToString(),
                Priority = Priority.Medium,
                IsDone = false
            };
        }

        private static SimpleTask CreateSimpleTask(Guid id, Priority priority = Priority.Medium, bool isDone = false)
        {
            var simpleTaskDTO = CreateSimpleTaskDTO(priority);
            return new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, isDone);
        }
    }
}
