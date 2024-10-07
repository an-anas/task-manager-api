using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models;
using UpdateResult = TaskManager.Models.UpdateResult;

namespace TaskManager.DataAccess.Repository
{
    [ExcludeFromCodeCoverage]
    public class TaskItemRepository(IMongoDbContext context) : ITaskItemRepository
    {
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId, bool? completed)
        {
            var filter = Builders<TaskItem>.Filter.Eq(task => task.UserId, userId);

            if (completed.HasValue)
            {
                filter &= Builders<TaskItem>.Filter.Eq(task => task.Completed, completed.Value);
            }

            return await context.TaskItems.Find(filter).ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(string taskId, string userId)
        {
            var filter = Builders<TaskItem>.Filter.Eq(task => task.Id, taskId) &
                         Builders<TaskItem>.Filter.Eq(task => task.UserId, userId);

            return await context.TaskItems.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddTaskAsync(TaskItem task)
        {
            await context.TaskItems.InsertOneAsync(task);
        }

        public async Task<UpdateResult> UpdateTaskAsync(string taskId, string userId, TaskItem updatedTask)
        {
            var filter = Builders<TaskItem>.Filter.Eq(task => task.Id, taskId) &
                         Builders<TaskItem>.Filter.Eq(task => task.UserId, userId);

            var result = await context.TaskItems.ReplaceOneAsync(filter, updatedTask);

            return new UpdateResult
            {
                Found = result.MatchedCount > 0,
                Updated = result.ModifiedCount > 0
            };
        }

        public async Task<bool> DeleteTaskAsync(string id, string userId)
        {
            var filter = Builders<TaskItem>.Filter.Eq(task => task.Id, id) &
                         Builders<TaskItem>.Filter.Eq(task => task.UserId, userId);

            return (await context.TaskItems.DeleteOneAsync(filter)).DeletedCount != 0;
        }
    }
}
