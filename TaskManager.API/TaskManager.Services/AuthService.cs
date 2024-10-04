using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class AuthService(IConfiguration configuration) : IAuthService
    {
        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(computedHash) == storedHash;
        }

        public (string PasswordHash, string PasswordSalt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = Convert.ToBase64String(hmac.Key);
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return (passwordHash, passwordSalt);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"] ?? throw new InvalidOperationException("The Jwt:Secret configuration key must be set."));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]
                                              ?? throw new InvalidOperationException(
                                                  "The Jwt:Secret configuration key must be set."));

            // Set validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Set true if you want to validate the issuer
                ValidateAudience = false, // Set true if you want to validate the audience
                ClockSkew = TimeSpan.Zero // Set to zero to avoid delay in token expiration
            };

            try
            {
                // Validate the token and extract claims
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (Exception ex)
            {
                if (ex is SecurityTokenExpiredException)
                {
                    throw new SecurityTokenExpiredException("Token has expired.");
                }

                // Handle token validation failure (e.g., logging, rethrowing, etc.)
                throw new SecurityTokenException("Invalid token.");
            }
        }

    }
}
