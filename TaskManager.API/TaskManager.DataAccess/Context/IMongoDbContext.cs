using MongoDB.Driver;
using TaskManager.Models;

namespace TaskManager.DataAccess.Context
{
    public interface IMongoDbContext
    {
        IMongoCollection<TaskItem> TaskItems { get; }
    }
}