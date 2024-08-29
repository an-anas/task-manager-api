using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models;

namespace TaskManager.DataAccess.Repository
{
    public class TaskItemRepository(IMongoDbContext context) : ITaskItemRepository
    {
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await context.TaskItems.Find(_ => true).ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(string id)
        {
            return await context.TaskItems.Find(task => task.Id == id).FirstOrDefaultAsync();
        }
    }
}
