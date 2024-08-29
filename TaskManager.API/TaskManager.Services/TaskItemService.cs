using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskItemService(ITaskItemRepository repository) : ITaskItemService
    {
        public async Task<Dictionary<bool, List<TaskItem>>> GetAllTasksByCompletedStatusAsync()
        {
            var tasks = await repository.GetAllTasksAsync();
            var taskItems = tasks as TaskItem[] ?? tasks.ToArray();

            var response = new Dictionary<bool, List<TaskItem>>()
                {
                    { true, taskItems.Where(task => task.Completed == true).ToList() },
                    { false, taskItems.Where(task => task.Completed == false).ToList() }
                };

            return response;
        }
    }
}
