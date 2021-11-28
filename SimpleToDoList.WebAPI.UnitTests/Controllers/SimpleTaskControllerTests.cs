using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using SimpleToDoList.Application.DTOs;
using SimpleToDoList.Application.Interfaces;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;
using SimpleToDoList.WebAPI.Controllers;
using Xunit;

namespace SimpleToDoList.WebAPI.UnitTests.Controllers
{
    public class SimpleTaskControllerTests
    {
        private readonly SimpleTaskController _simpleTaskController;
        private readonly ISimpleTaskService _simpleTaskService;
        private readonly INotificationContext _notification;
        private readonly LinkGenerator _linkGenerator;

        public SimpleTaskControllerTests()
        {
            _simpleTaskService = Substitute.For<ISimpleTaskService>();
            _linkGenerator = Substitute.For<LinkGenerator>();
            var logger = Substitute.For<ILogger<SimpleTaskController>>();
            _notification = Substitute.For<INotificationContext>();
            
            _simpleTaskController = new SimpleTaskController(_simpleTaskService, _linkGenerator, logger, _notification)
            {
                ControllerContext =
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }
        
        [Fact]
        public async Task Create_ShouldReturnStatus201Created_WhenValidSimpleTaskDTO()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            var simpleTask = new SimpleTask(Guid.NewGuid(), simpleTaskDTO.Description, simpleTaskDTO.Priority, false);

            _notification.IsValid.Returns(true);
            _simpleTaskService.CreateAsync(simpleTaskDTO, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Create(simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var simpleTaskResponseDTO = (SimpleTaskResponseDTO)objectResult.Value;
            objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            simpleTaskResponseDTO.Description.Should().Be(simpleTaskDTO.Description);
            simpleTaskResponseDTO.Priority.Should().Be(simpleTaskDTO.Priority);
            simpleTaskResponseDTO.IsDone.Should().Be(simpleTask.IsDone);
        }
        
        [Fact]
        public async Task Create_ShouldReturnLinks_WhenValidSimpleTaskDTO()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            var id = Guid.NewGuid();
            var simpleTask = new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, false);
            var link = $"foo/{id}";

            _linkGenerator.GetUriByAddress(_simpleTaskController.HttpContext, Arg.Any<RouteValuesAddress>(), Arg.Any<RouteValueDictionary>(), default, default, default, default, default, default)
                .Returns(link);

            _notification.IsValid.Returns(true);
            _simpleTaskService.CreateAsync(simpleTaskDTO, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Create(simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var simpleTaskResponseDTO = (SimpleTaskResponseDTO)objectResult.Value;
            simpleTaskResponseDTO.Links.Single(x => x.Action.Equals("PUT") && x.Href.Equals(link) && x.Rel.Equals("self")).Should().NotBeNull();
            simpleTaskResponseDTO.Links.Single(x => x.Action.Equals("DELETE") && x.Href.Equals(link) && x.Rel.Equals("self")).Should().NotBeNull();
            simpleTaskResponseDTO.Links.Single(x => x.Action.Equals("GET") && x.Href.Equals(link) && x.Rel.Equals("self")).Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldReturnStatus400BadRequest_WhenInvalidSimpleTaskDTO()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            simpleTaskDTO.Description = string.Empty;
            var simpleTask = new SimpleTask(Guid.NewGuid(), simpleTaskDTO.Description, simpleTaskDTO.Priority, false);
            var notificationMessage = new NotificationMessage("Missing Mandatory Parameter", "Description is mandatory");

            _notification.IsValid.Returns(false);
            _notification.Notifications
                .Returns(new List<NotificationMessage> { notificationMessage });
            _simpleTaskService.CreateAsync(simpleTaskDTO, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Create(simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var notifications = (List<NotificationMessage>)objectResult.Value;
            objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            notifications.First().Message.Should().Be(notificationMessage.Message);
        }

        [Fact]
        public async Task GetAll_ShouldReturnStatus200OK_WhenTasksExist()
        {
            // Arrange
            var query = new SimpleTaskQuery();
            var simpleTaskDTO = CreateSimpleTaskDTO();
            var simpleTask = new SimpleTask(Guid.NewGuid(), simpleTaskDTO.Description, simpleTaskDTO.Priority, false);

            _notification.IsValid.Returns(true);
            _simpleTaskService.GetAllAsync(query, Arg.Any<CancellationToken>())
                .Returns(new List<SimpleTask> { simpleTask });
            
            // Act
            var result = await _simpleTaskController.GetAll(query, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var tasks = (List<SimpleTaskResponseDTO>)objectResult.Value;
            objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            tasks.First().Description.Should().Be(simpleTaskDTO.Description);
        }
        
        [Fact]
        public async Task GetAll_ShouldReturnStatus204NoContent_WhenTasksDontExist()
        {
            // Arrange
            var query = new SimpleTaskQuery();
            _notification.IsValid.Returns(true);
            _simpleTaskService.GetAllAsync(query, Arg.Any<CancellationToken>())
                .ReturnsNull();
            
            // Act
            var result = await _simpleTaskController.GetAll(query, new CancellationToken());
            
            // Assert
            var objectResult = (NoContentResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
        
        [Fact]
        public async Task Get_ShouldReturnStatus200OK_WhenTaskExists()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            var id = Guid.NewGuid();
            var simpleTask = new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, false);

            _notification.IsValid.Returns(true);
            _simpleTaskService.GetAsync(id, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Get(id, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var task = (SimpleTaskResponseDTO)objectResult.Value;
            objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            task.Description.Should().Be(simpleTaskDTO.Description);
        }
        
        [Fact]
        public async Task Get_ShouldReturnStatus404NotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var notificationMessage = new NotificationMessage("Task not found", $"There is no task with id = {id}");
            
            _notification.IsValid.Returns(false);
            _notification.Notifications
                .Returns(new List<NotificationMessage> { notificationMessage });
            _simpleTaskService.GetAsync(id, Arg.Any<CancellationToken>())
                .ReturnsNull();
            
            // Act
            var result = await _simpleTaskController.Get(id, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task Update_ShouldReturnStatus200OK_WhenValidSimpleTaskDTO()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();
            var simpleTask = new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, simpleTaskDTO.IsDone);

            _notification.IsValid.Returns(true);
            _simpleTaskService.UpdateAsync(id, simpleTaskDTO, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Update(id, simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            var simpleTaskResponseDTO = (SimpleTaskResponseDTO)objectResult.Value;
            objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            simpleTaskResponseDTO.Description.Should().Be(simpleTaskDTO.Description);
            simpleTaskResponseDTO.Priority.Should().Be(simpleTaskDTO.Priority);
            simpleTaskResponseDTO.IsDone.Should().Be(simpleTask.IsDone);
        }
        
        [Fact]
        public async Task Update_ShouldReturnStatus404NotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();
            var notificationMessage = new NotificationMessage("Task not found", $"There is no task with id = {id}");
            
            _notification.IsValid.Returns(false);
            _notification.Notifications
                .Returns(new List<NotificationMessage> { notificationMessage });
            _simpleTaskService.UpdateAsync(id, simpleTaskDTO, Arg.Any<CancellationToken>())
                .ReturnsNull();
            
            // Act
            var result = await _simpleTaskController.Update(id, simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task Delete_ShouldReturnStatus204NoContent_WhenTaskExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();
            var simpleTask = new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, simpleTaskDTO.IsDone);

            _notification.IsValid.Returns(true);
            _simpleTaskService.UpdateAsync(id, simpleTaskDTO, Arg.Any<CancellationToken>())
                .Returns(simpleTask);
            
            // Act
            var result = await _simpleTaskController.Delete(id, new CancellationToken());
            
            // Assert
            var objectResult = (NoContentResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
        
        [Fact]
        public async Task Delete_ShouldReturnStatus404NotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var simpleTaskDTO = CreateSimpleTaskUpdateDTO();
            var notificationMessage = new NotificationMessage("Task not found", $"There is no task with id = {id}");
            
            _notification.IsValid.Returns(false);
            _notification.Notifications
                .Returns(new List<NotificationMessage> { notificationMessage });
            _simpleTaskService.UpdateAsync(id, simpleTaskDTO, Arg.Any<CancellationToken>())
                .ReturnsNull();
            
            // Act
            var result = await _simpleTaskController.Delete(id, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task NotificationResponseControl_ShouldReturnStatusStatus500InternalServerError_WhenThrowsException()
        {
            // Arrange
            var simpleTaskDTO = CreateSimpleTaskDTO();
            simpleTaskDTO.Description = string.Empty;
            _simpleTaskService.CreateAsync(simpleTaskDTO, Arg.Any<CancellationToken>())
                .Throws(new Exception());
            
            // Act
            var result = await _simpleTaskController.Create(simpleTaskDTO, new CancellationToken());
            
            // Assert
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
        
        private static SimpleTaskDTO CreateSimpleTaskDTO()
        {
            return new SimpleTaskDTO
            {
                Description = Guid.NewGuid().ToString(),
                Priority = Priority.Medium,
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
    }
}
