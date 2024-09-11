using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models;
using UpdateResult = TaskManager.Models.UpdateResult;

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

        public async Task<UpdateResult> UpdateTaskAsync(string id, TaskItem updatedTask)
        {
            var result = await context.TaskItems.ReplaceOneAsync(task => task.Id == id, updatedTask);

            return new UpdateResult
            {
                Found = result.MatchedCount > 0,
                Updated = result.ModifiedCount > 0
            };
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            var result = await context.TaskItems.DeleteOneAsync(task => task.Id == id);

            return result.DeletedCount != 0;
        }
    }
}
