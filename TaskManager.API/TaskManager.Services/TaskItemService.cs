using TaskManager.DataAccess.Repository;
using TaskManager.Models;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class TaskItemService(ITaskItemRepository repository) : ITaskItemService
    {
        public Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId, bool? completed)
        {
            return repository.GetAllTasksAsync(userId, completed);
        }

        public Task<TaskItem?> GetTaskByIdAsync(string taskId, string userId)
        {
            return repository.GetTaskByIdAsync(taskId, userId);
        }

        public Task AddTaskAsync(TaskItem task)
        {
            return repository.AddTaskAsync(task);
        }

        public Task<UpdateResult> UpdateTaskAsync(string taskId, string userId, TaskItem updatedTask)
        {
            return repository.UpdateTaskAsync(taskId, userId, updatedTask);
        }

        public Task<bool> DeleteTaskAsync(string taskId, string userId)
        {
            return repository.DeleteTaskAsync(taskId, userId);
        }
    }
}
