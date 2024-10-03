using TaskManager.DataAccess.Repository;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class UserService(IUserRepository repository) : IUserService
    {
        public Task<User?> GetUserByIdAsync(string userId)
        {
            return repository.GetUserByIdAsync(userId);
        }

        public Task<string?> LoginAsync(UserLoginDto loginDto)
        {
            return repository.LoginAsync(loginDto);
        }

        public Task<User> RegisterAsync(UserRegistrationDto registrationDto)
        {
            return repository.RegisterAsync(registrationDto);
        }
    }
}
