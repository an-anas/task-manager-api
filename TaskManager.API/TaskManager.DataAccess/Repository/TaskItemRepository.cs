using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models;

namespace TaskManager.DataAccess.Repository
{
    public class TaskItemRepository(IMongoDbContext context) : ITaskItemRepository
    {
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(bool? completed)
        {
            if (completed.HasValue)
            {
                return await context.TaskItems.Find(task => task.Completed == completed).ToListAsync();
            }

            return await context.TaskItems.Find(task => true).ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(string id)
        {
            return await context.TaskItems.Find(task => task.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddTaskAsync(TaskItem task)
        {
            await context.TaskItems.InsertOneAsync(task);
        }

        public async Task<bool> UpdateTaskAsync(string id, TaskItem updatedTask)
        {
            var result = await context.TaskItems.ReplaceOneAsync(task => task.Id == id, updatedTask);

            return result.ModifiedCount != 0;
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            var result = await context.TaskItems.DeleteOneAsync(task => task.Id == id);

            return result.DeletedCount != 0;
        }
    }
}
