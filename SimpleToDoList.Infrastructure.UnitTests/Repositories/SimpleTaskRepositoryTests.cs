using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;
using SimpleToDoList.Infrastructure.Repositories;
using Xunit;

namespace SimpleToDoList.Infrastructure.UnitTests.Repositories
{
    public class SimpleTaskRepositoryTests
    {
        private readonly IDatabaseFileProvider _database = Substitute.For<IDatabaseFileProvider>();
        private readonly SimpleTaskRepository _simpleTaskRepository;
        private const string FileName = "SimpleTask";

        public SimpleTaskRepositoryTests()
        {
            _simpleTaskRepository = new SimpleTaskRepository(_database);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTask_WhenSimpleTaskValid()
        {
            // Arrange
            var simpleTask = CreateSimpleTask(Guid.NewGuid());

            // Act
            await _simpleTaskRepository.CreateAsync(simpleTask, new CancellationToken());

            // Assert
            await _database.Received(1).SaveAsync(FileName, Arg.Any<ICollection<SimpleTask>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAsync_ShouldGetTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(id);

            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });

            // Act
            var result = await _simpleTaskRepository.GetAsync(id, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().Be(simpleTask);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask>());

            // Act
            var result = await _simpleTaskRepository.GetAsync(id, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldGetAllTask_WhenTasksExists()
        {
            // Arrange
            var simpleTask1 = CreateSimpleTask(Guid.NewGuid());
            var simpleTask2 = CreateSimpleTask(Guid.NewGuid());

            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask1, simpleTask2 });

            // Act
            var result = await _simpleTaskRepository.GetAllAsync(new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().HaveCount(2);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldGetAllTasksDone_WhenTasksDoneExists()
        {
            // Arrange
            var simpleTask1 = CreateSimpleTask(Guid.NewGuid(), isDone: true);
            var simpleTask2 = CreateSimpleTask(Guid.NewGuid());

            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask1, simpleTask2 });

            // Act
            var result = await _simpleTaskRepository.GetAllAsync(new CancellationToken(), isDone: true);

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().HaveCount(1);
            result.First().Description.Should().Be(simpleTask1.Description);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldGetAllTasksPriorityMedium_WhenTasksPrioriMediumExists()
        {
            // Arrange
            var simpleTask1 = CreateSimpleTask(Guid.NewGuid(), priority: Priority.Medium);
            var simpleTask2 = CreateSimpleTask(Guid.NewGuid());

            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask1, simpleTask2 });

            // Act
            var result = await _simpleTaskRepository.GetAllAsync(new CancellationToken(), priority: Priority.Medium);

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().HaveCount(1);
            result.First().Description.Should().Be(simpleTask1.Description);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnNothing_WhenTasksDontExist()
        {
            // Arrange
            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask>());

            // Act
            var result = await _simpleTaskRepository.GetAllAsync(new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            result.Should().HaveCount(0);
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(id);
            
            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });

            // Act
            await _simpleTaskRepository.UpdateAsync(id, simpleTask, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            await _database.Received(1).SaveAsync(FileName, Arg.Any<ICollection<SimpleTask>>(), Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldNotUpdateTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(Guid.NewGuid());
            
            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });

            // Act
            await _simpleTaskRepository.UpdateAsync(id, simpleTask, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            await _database.DidNotReceive().SaveAsync(FileName, Arg.Any<ICollection<SimpleTask>>(), Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task DeleteAsync_ShouldDeleteTask_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(id);
            
            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });

            // Act
            await _simpleTaskRepository.DeleteAsync(id, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            await _database.Received(1).SaveAsync(FileName, Arg.Any<ICollection<SimpleTask>>(), Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task DeleteAsync_ShouldNotDeleteTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTask = CreateSimpleTask(Guid.NewGuid());
            
            _database.LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });

            // Act
            await _simpleTaskRepository.DeleteAsync(id, new CancellationToken());

            // Assert
            await _database.Received(1).LoadAsync<SimpleTask>(FileName, Arg.Any<CancellationToken>());
            await _database.DidNotReceive().SaveAsync(FileName, Arg.Any<ICollection<SimpleTask>>(), Arg.Any<CancellationToken>());
        }
        
        private static SimpleTask CreateSimpleTask(Guid id, Priority priority = Priority.High, bool isDone = false)
        {
            return new SimpleTask(id, Guid.NewGuid().ToString(), priority, isDone);
        }
    }
}
