using TaskManager.DataAccess.Repository;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class UserService(IUserRepository userRepository, IAuthService authService) : IUserService
    {
        public async Task<string?> LoginAsync(UserLoginDto loginDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(loginDto.Username);
            if (user == null || !authService.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return authService.GenerateJwtToken(user);
        }

        public async Task<User> RegisterAsync(UserRegistrationDto registrationDto)
        {
            var existingUserByUsername = await userRepository.GetUserByUsernameAsync(registrationDto.Username);
            var existingUserByEmail = await userRepository.GetUserByEmailAsync(registrationDto.Email);

            if (existingUserByUsername != null)
            {
                throw new InvalidOperationException("This username is already taken.");
            }

            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException("This email is already registered to a different account.");
            }

            var (passwordHash, passwordSalt) = authService.HashPassword(registrationDto.Password);
            var newUser = new User
            {
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await userRepository.AddUserAsync(newUser);
            return newUser;
        }
    }
}