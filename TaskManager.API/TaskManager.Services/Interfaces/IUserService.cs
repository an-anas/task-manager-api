using TaskManager.Models.Common;
using TaskManager.Models.User;

namespace TaskManager.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserLoginResponse?> LoginAsync(UserLoginDto model);
        Task<ResponseDto<UserRegistrationResponse>> RegisterAsync(UserRegistrationDto registrationDto);
        Task<UserLoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    }
}