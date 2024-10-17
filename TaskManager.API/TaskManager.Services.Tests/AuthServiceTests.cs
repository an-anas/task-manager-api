using System.IdentityModel.Tokens.Jwt;
using Moq;
using TaskManager.Common.Helpers;
using TaskManager.Models.Auth;
using TaskManager.Models.User;

namespace TaskManager.Services.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IConfigurationHelper> _configurationHelperMock;
        private AuthService _authService;

        [SetUp]
        public void SetUp()
        {
            _configurationHelperMock = new Mock<IConfigurationHelper>();

            // Mock the Jwt:Secret, Jwt:TokenExpirationInMinutes, and Jwt:RefreshTokenExpirationInDays settings
            _configurationHelperMock.Setup(config => config.GetConfigValue("Jwt:Secret"))
                .Returns("SuperSecretKeyThatIsAtLeast32CharactersLong");

            _configurationHelperMock.Setup(config => config.GetConfigValue("Jwt:TokenExpirationInMinutes"))
                .Returns("15");

            _configurationHelperMock.Setup(config => config.GetConfigValue("Jwt:RefreshTokenExpirationInDays"))
                .Returns("30");

            _authService = new AuthService(_configurationHelperMock.Object);
        }

        [Test]
        public void VerifyPassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "password123";
            var passwordHashModel = _authService.HashPassword(password);
            var model = new VerifyPasswordModel
            {
                Password = password,
                StoredHash = passwordHashModel.Hash,
                StoredSalt = passwordHashModel.Salt
            };

            // Act
            var result = _authService.VerifyPassword(model);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var password = "password123";
            var passwordHashModel = _authService.HashPassword(password);
            var model = new VerifyPasswordModel
            {
                Password = "wrongpassword",
                StoredHash = passwordHashModel.Hash,
                StoredSalt = passwordHashModel.Salt
            };

            // Act
            var result = _authService.VerifyPassword(model);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HashPassword_ReturnsHashAndSalt()
        {
            // Arrange
            var password = "password123";

            // Act
            var passwordHashModel = _authService.HashPassword(password);
            var passwordHash = passwordHashModel.Hash;
            var passwordSalt = passwordHashModel.Salt;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(passwordHash, Is.Not.EqualTo(password));
                Assert.That(passwordSalt, Is.Not.EqualTo(password));
            });
        }

        [Test]
        public void GenerateTokens_ReturnsTokenResponseModel()
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

            // Act
            var tokenResponse = _authService.GenerateTokens(user);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(tokenResponse, Is.Not.Null);
                Assert.That(tokenResponse.AccessToken, Is.Not.Null);
                Assert.That(tokenResponse.RefreshToken, Is.Not.Null);
                Assert.That(tokenResponse.RefreshTokenExpirationDate,
                    Is.EqualTo(DateTime.UtcNow.AddDays(30)).Within(TimeSpan.FromSeconds(1)));
            });

            // Validate the access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(tokenResponse.AccessToken) as JwtSecurityToken;

            Assert.That(jwtToken, Is.Not.Null);

            // Check for unique_name (ClaimTypes.Name is serialized as unique_name in JWT)
            var uniqueNameClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            Assert.That(uniqueNameClaim, Is.Not.Null);
            Assert.That(uniqueNameClaim, Is.EqualTo(user.Username));

            // Check for nameid (ClaimTypes.NameIdentifier)
            var nameIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;
            Assert.That(nameIdClaim, Is.EqualTo(user.Id));
        }
    }
}
