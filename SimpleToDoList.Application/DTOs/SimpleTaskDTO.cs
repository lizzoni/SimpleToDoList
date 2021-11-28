using SimpleToDoList.Domain.Enums;

namespace SimpleToDoList.Application.DTOs
{
    public class SimpleTaskDTO
    {
        public string Description { get; set; }
        public Priority Priority { get; set; }
    }
}
