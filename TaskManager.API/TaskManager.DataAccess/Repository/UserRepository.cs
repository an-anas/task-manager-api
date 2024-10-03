using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.Models.User;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace TaskManager.DataAccess.Repository
{
    public class UserRepository(IMongoDbContext context, IConfiguration configuration) : IUserRepository
    {
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await context.Users.Find(user => user.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<string?> LoginAsync(UserLoginDto loginDto)
        {
            // Find user by username
            var user = await context.Users.Find(u => u.Username == loginDto.Username).FirstOrDefaultAsync();
            if (user == null) return null;

            // Validate password using the stored salt (passwordSalt)
            using var hmac = new HMACSHA512(Convert.FromBase64String(user.PasswordSalt));  // Use the stored salt
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            if (!Convert.ToBase64String(computedHash).SequenceEqual(user.PasswordHash))
            {
                return null; // Invalid password
            }

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<User> RegisterAsync(UserRegistrationDto registrationDto)
        {
            // Check if user already exists
            var existingUser = await context.Users.Find(u => u.Username == registrationDto.Username || u.Email == registrationDto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            // Generate salt and hash the password
            using var hmac = new HMACSHA512();
            var passwordSalt = Convert.ToBase64String(hmac.Key);  // Store the salt (key)
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registrationDto.Password)));

            // Create new user with both passwordHash and passwordSalt
            var newUser = new User
            {
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt  // Save the salt
            };

            await context.Users.InsertOneAsync(newUser);
            return newUser;
        }
    }
}
