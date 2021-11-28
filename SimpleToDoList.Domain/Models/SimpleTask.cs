using System;
using SimpleToDoList.Domain.Enums;

namespace SimpleToDoList.Domain.Models
{
    public class SimpleTask
    {
        public SimpleTask(Guid id, string description, Priority priority, bool isDone)
        {
            Id = id;
            Description = description;
            Priority = priority;
            IsDone = isDone;
        }

        public Guid Id { get; }
        public string Description { get; }
        public Priority Priority { get; }
        public bool IsDone { get; }
    }
}
