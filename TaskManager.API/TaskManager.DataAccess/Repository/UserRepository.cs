using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models.User;

namespace TaskManager.DataAccess.Repository
{
    [ExcludeFromCodeCoverage]
    public class UserRepository(IMongoDbContext context) : IUserRepository
    {
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await context.Users.Find(user => user.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task AddUserAsync(User newUser)
        {
            await context.Users.InsertOneAsync(newUser);
        }
    }
}
