using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using TaskManager.Models;
using TaskManager.Models.User;

namespace TaskManager.DataAccess.Context
{
    [ExcludeFromCodeCoverage]
    public class MongoDbContext(IMongoClient mongoClient, string databaseName) : IMongoDbContext
    {
        private readonly IMongoDatabase _database = mongoClient.GetDatabase(databaseName);

        public IMongoCollection<TaskItem> TaskItems => _database.GetCollection<TaskItem>("tasks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    }
}