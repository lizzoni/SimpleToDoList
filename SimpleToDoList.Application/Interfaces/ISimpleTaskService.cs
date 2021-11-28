using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleToDoList.Application.DTOs;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Application.Interfaces
{
    public interface ISimpleTaskService
    {
        Task<SimpleTask> CreateAsync(SimpleTaskDTO simpleTaskDTO, CancellationToken token);
        Task<SimpleTask> GetAsync(Guid id, CancellationToken token);
        Task<ICollection<SimpleTask>> GetAllAsync(SimpleTaskQuery simpleTaskQuery, CancellationToken token);
        Task<SimpleTask> UpdateAsync(Guid id, SimpleTaskUpdateDTO simpleTaskDTO, CancellationToken token);
        Task<object> DeleteAsync(Guid id, CancellationToken token);
    }
}
