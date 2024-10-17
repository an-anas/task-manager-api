using TaskManager.Models.Auth;
using TaskManager.Models.User;

namespace TaskManager.Services.Interfaces
{
    public interface IAuthService
    {
        bool VerifyPassword(VerifyPasswordModel model);
        PasswordHashModel HashPassword(string password);
        TokenResponseModel GenerateTokens(User user);
    }
}