using Microsoft.AspNetCore.Mvc;
using TaskManager.DataAccess.Repository;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskItemsController(ILogger<TaskItemsController> logger, ITaskItemRepository repository) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var tasks = repository.GetAllTasksAsync().Result;

            return Ok(tasks); // Returns a 200 OK response with the tasks in the response body
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var task = repository.GetTaskByIdAsync(id).Result;

            if (task == null)
            {
                return NotFound(); // Returns a 404 Not Found response
            }

            return Ok(task); // Returns a 200 OK response with the task in the response body
        }
    }
}