using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleToDoList.Application.DTOs;
using SimpleToDoList.Application.Interfaces;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Application.Services
{
    public class SimpleTaskService : ISimpleTaskService
    {
        private readonly ISimpleTaskRepository _simpleTaskRepository;
        private readonly INotificationContext _notification;
        private readonly ILogger<SimpleTaskService> _logger;

        public SimpleTaskService(ISimpleTaskRepository simpleTaskRepository, INotificationContext notification, ILogger<SimpleTaskService> logger)
        {
            _simpleTaskRepository = simpleTaskRepository;
            _notification = notification;
            _logger = logger;
        }

        public async Task<SimpleTask> CreateAsync(SimpleTaskDTO simpleTaskDTO, CancellationToken token)
        {
            _logger.LogDebug("SimpleTaskService.CreateAsync");
            ValidateRequest(simpleTaskDTO);

            if (!_notification.IsValid)
                return null;

            var task = new SimpleTask(Guid.NewGuid(), simpleTaskDTO.Description, simpleTaskDTO.Priority, false);

            await _simpleTaskRepository.CreateAsync(task, token);
            return task;
        }

        public async Task<SimpleTask> GetAsync(Guid id, CancellationToken token)
        {
            _logger.LogDebug("SimpleTaskService.GetAsync");
            var task = await _simpleTaskRepository.GetAsync(id, token);
            if (task != null)
                return task;

            _notification.AddNotification("Task not found", $"There is no task with id = {id}");
            return null;
        }

        public async Task<ICollection<SimpleTask>> GetAllAsync(SimpleTaskQuery simpleTaskQuery, CancellationToken token)
        {
            _logger.LogDebug("SimpleTaskRepository.GetAllAsync");
            var tasks = await _simpleTaskRepository.GetAllAsync(token, simpleTaskQuery.IsDone, simpleTaskQuery.Priority);
            return tasks?.Count == 0 ? null : tasks;
        }

        public async Task<SimpleTask> UpdateAsync(Guid id, SimpleTaskUpdateDTO simpleTaskDTO, CancellationToken token)
        {
            _logger.LogDebug("SimpleTaskService.UpdateAsync");
            ValidateRequest(simpleTaskDTO);

            if (!_notification.IsValid)
                return null;

            var task = await GetAsync(id, token);
            if (task == null)
                return null;
            var updateTask = new SimpleTask(id, simpleTaskDTO.Description, simpleTaskDTO.Priority, simpleTaskDTO.IsDone);
            await _simpleTaskRepository.UpdateAsync(id, updateTask, token);
            return updateTask;
        }

        public async Task<object> DeleteAsync(Guid id, CancellationToken token)
        {
            _logger.LogDebug("SimpleTaskService.DeleteAsync");
            var task = await GetAsync(id, token);
            if (task == null)
                return null;

            await _simpleTaskRepository.DeleteAsync(id, token);
            return null;
        }

        private void ValidateRequest(SimpleTaskDTO simpleTaskDTO)
        {
            if (string.IsNullOrEmpty(simpleTaskDTO.Description))
                _notification.AddNotification("Missing Mandatory Parameter", "Description is mandatory");
            if (simpleTaskDTO.Priority == 0)
                _notification.AddNotification("Missing Mandatory Parameter", "Priority is mandatory");
            else if (!Enum.IsDefined(typeof(Priority), simpleTaskDTO.Priority))
                _notification.AddNotification("Invalid Parameter Value", "Invalid Priority");
        }
    }
}
