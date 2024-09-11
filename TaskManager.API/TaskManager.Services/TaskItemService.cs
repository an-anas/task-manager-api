using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskItemService(ITaskItemRepository repository) : ITaskItemService
    {
        public Task<IEnumerable<TaskItem>> GetAllTasksAsync(bool? completed)
        {
            return repository.GetAllTasksAsync(completed);
        }

        public Task<TaskItem?> GetTaskByIdAsync(string id)
        {
            return repository.GetTaskByIdAsync(id);
        }

        public Task AddTaskAsync(TaskItem task)
        {
            return repository.AddTaskAsync(task);
        }

        public Task<UpdateResult> UpdateTaskAsync(string id, TaskItem updatedTask)
        {
            return repository.UpdateTaskAsync(id, updatedTask);
        }

        public Task<bool> DeleteTaskAsync(string id)
        {
            return repository.DeleteTaskAsync(id);
        }
    }
}
