using SimpleToDoList.Domain.Enums;

namespace SimpleToDoList.Application.DTOs
{
    public class SimpleTaskQuery
    {
        public bool? IsDone { get; set; }
        public Priority? Priority { get; set; }
    }
}
