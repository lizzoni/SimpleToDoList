using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Domain.Interfaces
{
    public interface ISimpleTaskRepository
    {
        Task CreateAsync(SimpleTask simpleTask, CancellationToken token);
        Task<SimpleTask> GetAsync(Guid id, CancellationToken token);
        Task<ICollection<SimpleTask>> GetAllAsync(CancellationToken token, bool? isDone = null, Priority? priority = null);
        Task UpdateAsync(Guid id, SimpleTask simpleTask, CancellationToken token);
        Task DeleteAsync(Guid id, CancellationToken token);
    }
}
