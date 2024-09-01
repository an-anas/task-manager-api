using TaskManager.Models;

namespace TaskManager.DataAccess.Repository
{
    public interface ITaskItemRepository
    {
        // Retrieve all tasks
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(bool? completed);

        // Retrieve a task by its ID
        Task<TaskItem?> GetTaskByIdAsync(string id);

        //// Add a new task
        Task AddTaskAsync(TaskItem task);

        //// Update an existing task
        Task<bool> UpdateTaskAsync(string id, TaskItem updatedTask);

        //// Delete a task by its ID
        Task<bool> DeleteTaskAsync(string id);
    }
}

