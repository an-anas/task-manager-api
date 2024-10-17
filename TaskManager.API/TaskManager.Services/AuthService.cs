using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common.Helpers;
using TaskManager.Models.Auth;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class AuthService : IAuthService
    {
        private readonly int _tokenExpirationInMinutes;
        private readonly int _refreshTokenExpirationInDays;
        private readonly IConfigurationHelper _configHelper;

        public AuthService(IConfigurationHelper configHelper)
        {
            _configHelper = configHelper;
            _tokenExpirationInMinutes = int.Parse(_configHelper.GetConfigValue("Jwt:TokenExpirationInMinutes"));
            _refreshTokenExpirationInDays = int.Parse(_configHelper.GetConfigValue("Jwt:RefreshTokenExpirationInDays"));
        }

        public bool VerifyPassword(VerifyPasswordModel model)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(model.StoredSalt));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
            return Convert.ToBase64String(computedHash) == model.StoredHash;
        }

        public PasswordHashModel HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = Convert.ToBase64String(hmac.Key);
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return new PasswordHashModel
            {
                Hash = passwordHash,
                Salt = passwordSalt
            };
        }

        public TokenResponseModel GenerateTokens(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            return new TokenResponseModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(_refreshTokenExpirationInDays)
            };
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configHelper.GetConfigValue("Jwt:Secret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                    new Claim(ClaimTypes.Name, user.Username)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(_tokenExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
