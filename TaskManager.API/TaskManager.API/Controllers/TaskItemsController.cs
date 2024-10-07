using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Services.Interfaces;

namespace TaskManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TaskItemsController(
        ILogger<TaskItemsController> logger,
        ITaskItemService taskItemService)
        : ControllerBase
    {
        private string UserId => User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool? completed = null)
        {
            try
            {
                var tasks = await taskItemService.GetAllTasksAsync(UserId, completed);
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
                var task = await taskItemService.GetTaskByIdAsync(id, UserId);
                if (task == null)
                {
                    return NotFound();
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving task with ID {TaskId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TaskItem task)
        {
            try
            {
                task.UserId = UserId;

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

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
                var deleted = await taskItemService.DeleteTaskAsync(id, UserId);
                return deleted ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting task with ID {TaskId}.", id);
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
                var existingTask = await taskItemService.GetTaskByIdAsync(id, UserId);
                if (existingTask == null)
                {
                    return NotFound();
                }

                existingTask.Completed = completed.Value;

                var result = await taskItemService.UpdateTaskAsync(id, UserId, existingTask);

                if (!result.Found)
                {
                    return NotFound();
                }

                if (!result.Updated)
                {
                    return NoContent();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating task with ID {TaskId}.", id);
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

                var result = await taskItemService.UpdateTaskAsync(id, UserId, updatedTask);
                if (!result.Found)
                {
                    return NotFound();
                }

                if (!result.Updated)
                {
                    return NoContent();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating task with ID {TaskId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
