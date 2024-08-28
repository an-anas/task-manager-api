using Microsoft.AspNetCore.Mvc;
using Task = TaskManager.API.Models.Task;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController(ILogger<TasksController> logger) : ControllerBase
    {
        private readonly ILogger<TasksController> _logger = logger;

        [HttpGet]
        public IEnumerable<Task> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Task
            {
                Id = "test-id",
                Title = "Trash",
                Description = "Take out the trash",
                Completed = false
            }).ToArray();
        }
    }
}