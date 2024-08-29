using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TaskManager.Models;

namespace TaskManager.DataAccess.Context
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase(configuration["DatabaseSettings:DatabaseName"]);
        }

        public IMongoCollection<TaskItem> TaskItems => _database.GetCollection<TaskItem>("tasks");
    }
}