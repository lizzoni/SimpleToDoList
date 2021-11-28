using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Models;
using SimpleToDoList.Infrastructure.Providers;
using Xunit;

namespace SimpleToDoList.Infrastructure.UnitTests.Providers
{
    public class JsonFileProviderTests
    {
        private readonly JsonFileProvider _jsonFileProvider;

        public JsonFileProviderTests()
        {
            _jsonFileProvider = new JsonFileProvider("Database");
        }

        [Fact]
        public async Task LoadAsync_ShouldLoadTasks_WhenFileExists()
        {
            // Act
            var result = await _jsonFileProvider.LoadAsync<SimpleTask>("test", new CancellationToken());

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task LoadAsync_ShouldNotLoadTasks_WhenFileDoesNotExist()
        {
            // Act
            var result = await _jsonFileProvider.LoadAsync<SimpleTask>("test2", new CancellationToken());

            // Assert
            result.Should().HaveCount(0);
        }

        [Fact]
        public async Task SaveAsync_ShouldSaveTasks_WhenTasksExist()
        {
            // Arrange
            var simpleTask1 = CreateSimpleTask(Guid.NewGuid());
            var simpleTask2 = CreateSimpleTask(Guid.NewGuid());
            var tasks = new List<SimpleTask> { simpleTask1, simpleTask2 };

            // Act
            await _jsonFileProvider.SaveAsync("test_save", tasks, new CancellationToken());
            var result = await _jsonFileProvider.LoadAsync<SimpleTask>("test_save", new CancellationToken());
            File.Delete("test_save.json");

            // Assert
            result.Should().HaveCount(2);
        }

        private static SimpleTask CreateSimpleTask(Guid id)
        {
            return new SimpleTask(id, Guid.NewGuid().ToString(), Priority.High, false);
        }
    }
}
