using MongoDB.Driver;
using TaskManager.Models;
using TaskManager.Models.User;

namespace TaskManager.DataAccess.Context
{
    public interface IMongoDbContext
    {
        IMongoCollection<TaskItem> TaskItems { get; }
        IMongoCollection<User> Users { get; }
    }
}