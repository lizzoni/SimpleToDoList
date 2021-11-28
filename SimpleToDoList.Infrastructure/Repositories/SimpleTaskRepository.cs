using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.Infrastructure.Repositories
{
    public class SimpleTaskRepository : ISimpleTaskRepository
    {
        private readonly IDatabaseFileProvider _databaseFileProvider;
        private const string FileName = "SimpleTask";

        public SimpleTaskRepository(IDatabaseFileProvider databaseFileProvider)
        {
            _databaseFileProvider = databaseFileProvider;
        }

        public async Task CreateAsync(SimpleTask simpleTask, CancellationToken token)
        {
            var tasks = await GetAllAsync(token);
            tasks.Add(simpleTask);
            await _databaseFileProvider.SaveAsync(FileName, tasks, token);
        }

        public async Task<SimpleTask> GetAsync(Guid id, CancellationToken token)
        {
            var tasks = await GetAllAsync(token);
            return tasks.FirstOrDefault(x => x.Id == id);
        }

        public async Task<ICollection<SimpleTask>> GetAllAsync(CancellationToken token, bool? isDone = null, Priority? priority = null)
        {
            var tasks = await _databaseFileProvider.LoadAsync<SimpleTask>(FileName, token);
            if (isDone.HasValue)
                tasks = tasks.Where(x => x.IsDone == isDone.Value).ToList();
            if (priority.HasValue)
                tasks = tasks.Where(x => x.Priority == priority.Value).ToList();
            return tasks;
        }

        public async Task UpdateAsync(Guid id, SimpleTask simpleTask, CancellationToken token)
        {
            var tasks = new List<SimpleTask>();
            tasks.AddRange(await GetAllAsync(token));
            var index = tasks.FindIndex(x => x.Id == id);
            if (index == -1)
                return;
            tasks[index] = simpleTask;
            await _databaseFileProvider.SaveAsync(FileName, tasks, token);
        }

        public async Task DeleteAsync(Guid id, CancellationToken token)
        {
            var tasks = new List<SimpleTask>();
            tasks.AddRange(await GetAllAsync(token));
            var index = tasks.FindIndex(x => x.Id == id);
            if (index == -1)
                return;
            tasks.Remove(tasks[index]);
            await _databaseFileProvider.SaveAsync(FileName, tasks, token);
        }
    }
}
