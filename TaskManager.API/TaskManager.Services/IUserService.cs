using TaskManager.Models.User;

namespace TaskManager.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(UserRegistrationDto registrationDto);
        Task<string?> LoginAsync(UserLoginDto loginDto);
        Task<User?> GetUserByIdAsync(string userId);
    }
}