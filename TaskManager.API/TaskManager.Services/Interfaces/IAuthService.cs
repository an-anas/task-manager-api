using TaskManager.Models.User;

namespace TaskManager.Services.Interfaces
{
    public interface IAuthService
    {
        bool VerifyPassword(string password, string storedHash, string storedSalt);
        (string PasswordHash, string PasswordSalt) HashPassword(string password);
        string GenerateJwtToken(User user);
    }
}
