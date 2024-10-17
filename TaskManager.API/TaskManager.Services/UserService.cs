using TaskManager.DataAccess.Repository;
using TaskManager.Models.Auth;
using TaskManager.Models.Common;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class UserService(IUserRepository userRepository, IAuthService authService) : IUserService
    {
        public async Task<UserLoginResponse?> LoginAsync(UserLoginDto loginDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                return null;
            }

            // Create the VerifyPasswordModel
            var verifyPasswordModel = new VerifyPasswordModel
            {
                Password = loginDto.Password,
                StoredHash = user.PasswordHash,
                StoredSalt = user.PasswordSalt
            };

            // Verify the password
            if (!authService.VerifyPassword(verifyPasswordModel))
            {
                return null;
            }

            // Generate tokens
            var tokenResponse = authService.GenerateTokens(user);

            // Store refresh token and expiration in the database
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiration = tokenResponse.RefreshTokenExpirationDate;
            await userRepository.UpdateUserAsync(user);

            return new UserLoginResponse
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken
            };
        }

        public async Task<ResponseDto<UserRegistrationResponse>> RegisterAsync(UserRegistrationDto registrationDto)
        {
            var existingUserByUsername = await userRepository.GetUserByUsernameAsync(registrationDto.Username);
            var existingUserByEmail = await userRepository.GetUserByEmailAsync(registrationDto.Email);

            if (existingUserByUsername != null)
            {
                return new ResponseDto<UserRegistrationResponse>
                {
                    Success = false,
                    ErrorMessage = "This username is already taken."
                };
            }

            if (existingUserByEmail != null)
            {
                return new ResponseDto<UserRegistrationResponse>
                {
                    Success = false,
                    ErrorMessage = "This email is already registered to a different account."
                };
            }

            var passwordHashModel = authService.HashPassword(registrationDto.Password);
            var newUser = new User
            {
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHashModel.Hash,
                PasswordSalt = passwordHashModel.Salt
            };

            await userRepository.AddUserAsync(newUser);

            return new ResponseDto<UserRegistrationResponse>
            {
                Success = true,
                Data = new UserRegistrationResponse
                {
                    Username = newUser.Username,
                    Email = newUser.Email
                }
            };
        }

        public async Task<UserLoginResponse?> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            // Get the user by the refresh token
            var user = await userRepository.GetUserByRefreshTokenAsync(refreshTokenRequest.RefreshToken);

            if (user == null || user.RefreshToken != refreshTokenRequest.RefreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                // If refresh token is invalid or expired, return null
                return null;
            }

            // Generate new tokens
            var tokenResponse = authService.GenerateTokens(user);

            // Update user's refresh token and expiration in the database
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiration = tokenResponse.RefreshTokenExpirationDate;
            await userRepository.UpdateUserAsync(user);

            return new UserLoginResponse
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken
            };
        }
    }
}
