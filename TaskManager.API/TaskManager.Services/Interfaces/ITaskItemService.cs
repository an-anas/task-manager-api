using TaskManager.Models;

namespace TaskManager.Services.Interfaces
{
    public interface ITaskItemService
    {
        // Retrieve all tasks
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId, bool? completed);

        // Retrieve a task by its ID
        Task<TaskItem?> GetTaskByIdAsync(string taskId, string userId);

        // Add a new task
        Task AddTaskAsync(TaskItem task);

        // Update an existing task
        Task<UpdateResult> UpdateTaskAsync(string id, string userId, TaskItem updatedTask);

        // Delete a task by its ID
        Task<bool> DeleteTaskAsync(string id, string userId);
    }
}
