using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SimpleToDoList.Application.DTOs;
using SimpleToDoList.Application.Interfaces;
using SimpleToDoList.Domain.Enums;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.WebAPI.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class SimpleTaskController : Controller
    {
        private readonly ISimpleTaskService _simpleTaskService;
        private readonly LinkGenerator _linkGenerator;

        public SimpleTaskController(
            ISimpleTaskService simpleTaskService,
            LinkGenerator linkGenerator,
            ILogger<SimpleTaskController> logger, 
            INotificationContext notification) : base(logger, notification)
        {
            _simpleTaskService = simpleTaskService;
            _linkGenerator = linkGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SimpleTaskDTO simpleTaskDTO, CancellationToken token)
        {
            return await NotificationResponseControl(
                async () => await _simpleTaskService.CreateAsync(simpleTaskDTO, token),
                AddSimpleTaskLinks,
                okActionResult: task => Created(GetUri(nameof(Get), ((SimpleTaskResponseDTO)task).Id), task));
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SimpleTaskQuery simpleTaskQuery, CancellationToken token)
        {
            return await NotificationResponseControl(
                async () => await _simpleTaskService.GetAllAsync(simpleTaskQuery, token),
                list =>
                {
                    var tasks = (IEnumerable<SimpleTask>)list;
                    return tasks.Select(AddSimpleTaskLinks).ToList();
                }
            );
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken token)
        {
            return await NotificationResponseControl(
                async () => await _simpleTaskService.GetAsync(id, token),
                AddSimpleTaskLinks,
                errorActionResult: NotFound);
        }
        
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SimpleTaskUpdateDTO simpleTaskDTO, CancellationToken token)
        {
            return await NotificationResponseControl(
                async () => await _simpleTaskService.UpdateAsync(id, simpleTaskDTO, token),
                AddSimpleTaskLinks,
                errorActionResult: NotFound);
        }
        
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken token)
        {
            return await NotificationResponseControl(
                async () => await _simpleTaskService.DeleteAsync(id, token),
                errorActionResult: NotFound);
        }

        private SimpleTaskResponseDTO AddSimpleTaskLinks(object task)
        {
            var taskDTO = task.Adapt<SimpleTaskResponseDTO>();
            taskDTO.Links.Add(new LinkDTO("self", GetUri(nameof(Get), taskDTO.Id), "GET"));
            taskDTO.Links.Add(new LinkDTO("self", GetUri(nameof(Delete), taskDTO.Id), "DELETE"));
            taskDTO.Links.Add(new LinkDTO("self", GetUri(nameof(Update), taskDTO.Id), "PUT"));
            return taskDTO;
        }
        
        private string GetUri(string action, Guid id) => _linkGenerator.GetUriByAction(HttpContext, action, values: new { id });
    }
}
