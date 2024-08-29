using TaskManager.Models;

namespace TaskManager.Services
{
    public interface ITaskItemService
    {
        Task<Dictionary<Boolean, List<TaskItem>>> GetAllTasksByCompletedStatusAsync();
    }
}
