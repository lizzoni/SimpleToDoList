using System;
using System.Collections.Generic;
using SimpleToDoList.Domain.Enums;

namespace SimpleToDoList.Application.DTOs
{
    public class SimpleTaskResponseDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public bool IsDone { get; set; }
        public IList<LinkDTO> Links { get; } = new List<LinkDTO>();
    }
}
