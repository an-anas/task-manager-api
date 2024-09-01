using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskItemsController(
        ILogger<TaskItemsController> logger,
        ITaskItemService taskItemService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool? completed = null)
        {
            try
            {
                var tasks = await taskItemService.GetAllTasksAsync(completed);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving tasks.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var task = await taskItemService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while retrieving task with ID {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await taskItemService.AddTaskAsync(task);
                return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the task.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await taskItemService.DeleteTaskAsync(id);
                return deleted ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while deleting task with ID {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("{id:length(24)}")]
        public async Task<IActionResult> Patch(string id, [FromQuery] bool? completed)
        {
            if (completed == null)
            {
                return BadRequest("The 'completed' query parameter is required.");
            }

            try
            {
                var existingTask = await taskItemService.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    return NotFound();
                }

                existingTask.Completed = completed.Value;

                var updated = await taskItemService.UpdateTaskAsync(id, existingTask);
                return updated ? Ok() : NoContent(); // NoContent if nothing has been modified
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while updating task with ID {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, [FromBody] TaskItem updatedTask)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                updatedTask.Id = id;

                var updated = await taskItemService.UpdateTaskAsync(id, updatedTask);
                return updated ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while updating task with ID {id}.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
