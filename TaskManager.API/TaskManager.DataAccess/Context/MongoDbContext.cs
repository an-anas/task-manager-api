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
            var connectionString = !string.IsNullOrEmpty(configuration.GetConnectionString("MongoDb"))
                ? configuration.GetConnectionString("MongoDb")
                : Environment.GetEnvironmentVariable("MongoDb__ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("MongoDb connection string is missing");
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(configuration["DatabaseSettings:DatabaseName"]);
        }

        public IMongoCollection<TaskItem> TaskItems => _database.GetCollection<TaskItem>("tasks");
    }
}