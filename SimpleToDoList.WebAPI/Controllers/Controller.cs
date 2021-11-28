using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Domain.Models;

namespace SimpleToDoList.WebAPI.Controllers
{
    public class Controller : ControllerBase
    {
        private readonly ILogger<Controller> _logger;
        private readonly INotificationContext _notificationContext;

        public Controller(ILogger<Controller> logger, INotificationContext notificationContext)
        {
            _logger = logger;
            _notificationContext = notificationContext;
        }

        protected async Task<ActionResult> NotificationResponseControl(
            Func<Task<object>> func, 
            Func<object, object> objectResult = null, 
            Func<object, ActionResult> okActionResult = null,
            Func<object, ActionResult> errorActionResult = null)
        {
            try
            {
                var result = await func.Invoke();

                if (!_notificationContext.IsValid) 
                    return errorActionResult != null ? errorActionResult.Invoke(_notificationContext.Notifications) : BadRequest(_notificationContext.Notifications);

                if (result == null)
                    return NoContent();

                if (objectResult != null)
                    result = objectResult.Invoke(result);
                
                return okActionResult != null ? okActionResult.Invoke(result) : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message + ", Stack trace: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + ", Stack trace: " + ex.StackTrace);
            }
        }
    }
}
