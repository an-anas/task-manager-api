using TaskManager.Models.User;

namespace TaskManager.DataAccess.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User newUser);
    }
}
