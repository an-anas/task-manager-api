using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TaskManager.Models.User;

namespace TaskManager.Services.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private AuthService _authService;

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<IConfiguration>();
            _authService = new AuthService(_configurationMock.Object);
        }

        // ...

        [Test]
        public void VerifyPassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "password123";
            var (storedHash, storedSalt) = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(password, storedHash, storedSalt);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var password = "password123";
            var (storedHash, storedSalt) = _authService.HashPassword(password);
            var incorrectPassword = "wrongpassword";

            // Act
            var result = _authService.VerifyPassword(incorrectPassword, storedHash, storedSalt);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HashPassword_ReturnsHashAndSalt()
        {
            // Arrange
            var password = "password123";

            // Act
            var (passwordHash, passwordSalt) = _authService.HashPassword(password);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(passwordHash, Is.Not.EqualTo(password));
                Assert.That(passwordSalt, Is.Not.EqualTo(password));
            });

        }

        [Test]
        public void GenerateJwtToken_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Id = "1",
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "dummyHash",
                PasswordSalt = "dummySalt"
            };
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns("SuperSecretKeyThatIsAtLeast32CharactersLong");

            // Act
            var token = _authService.GenerateJwtToken(user);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.TypeOf<string>());

            // Validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            Assert.That(jwtToken, Is.Not.Null);

            // Check for unique_name (ClaimTypes.Name is serialized as unique_name in JWT)
            var uniqueNameClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            Assert.That(uniqueNameClaim, Is.Not.Null);
            Assert.That(uniqueNameClaim, Is.EqualTo(user.Username));

            // Check for nameid (ClaimTypes.NameIdentifier)
            var nameIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;
            Assert.That(nameIdClaim, Is.EqualTo(user.Id));
        }


        [Test]
        public void GenerateJwtToken_MissingSecret_ThrowsInvalidOperationException()
        {
            // Arrange
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns((string?)null);

            var user = new User
            {
                Id = "1",
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "dummyHash",
                PasswordSalt = "dummySalt"
            };
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns((string?)null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _authService.GenerateJwtToken(user));
            Assert.That(ex.Message, Is.EqualTo("The Jwt:Secret configuration key must be set."));
        }

        [Test]
        public void ValidateJwtToken_ValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns("SuperSecretKeyThatIsAtLeast32CharactersLong");

            var user = new User
            {
                Id = "1",
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "dummyHash",
                PasswordSalt = "dummySalt"
            };

            // Create a valid token manually
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SuperSecretKeyThatIsAtLeast32CharactersLong");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                    new Claim(ClaimTypes.Name, user.Username)
                ]),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writtenToken = tokenHandler.WriteToken(token);

            // Act
            var claimsPrincipal = _authService.ValidateJwtToken(writtenToken);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(claimsPrincipal, Is.Not.Null);
                Assert.That(claimsPrincipal.Identity!.IsAuthenticated, Is.True);
                Assert.That(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value, Is.EqualTo("1"));
                Assert.That(claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value, Is.EqualTo("testuser"));
            });
        }

        [Test]
        public void ValidateJwtToken_InvalidToken_ThrowsSecurityTokenException()
        {
            // Arrange
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns("SuperSecretKeyThatIsAtLeast32CharactersLong");
            string invalidToken = "invalid.token.here";

            // Act & Assert
            var ex = Assert.Throws<SecurityTokenException>(() => _authService.ValidateJwtToken(invalidToken));
            Assert.That(ex.Message, Is.EqualTo("Invalid token."));
        }

        [Test]
        public void ValidateJwtToken_ExpiredToken_ThrowsSecurityTokenException()
        {
            // Arrange
            _configurationMock.Setup(config => config["Jwt:Secret"]).Returns("SuperSecretKeyThatIsAtLeast32CharactersLong");

            var user = new User
            {
                Id = "1",
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "dummyHash",
                PasswordSalt = "dummySalt"
            };

            // Generate a token with a short expiration time for testing
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SuperSecretKeyThatIsAtLeast32CharactersLong");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                    new Claim(ClaimTypes.Name, user.Username)
                ]),
                NotBefore = DateTime.UtcNow.AddSeconds(-60),
                Expires = DateTime.UtcNow, // Set token to expire immediately
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writtenToken = tokenHandler.WriteToken(token);

            // Act & Assert
            var ex = Assert.Throws<SecurityTokenExpiredException>(() => _authService.ValidateJwtToken(writtenToken));
            Assert.That(ex.Message, Is.EqualTo("Token has expired."));
        }

    }
}
