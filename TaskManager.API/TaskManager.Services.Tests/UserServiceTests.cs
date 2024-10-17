using Moq;
using TaskManager.DataAccess.Repository;
using TaskManager.Models.Auth;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IAuthService> _authServiceMock;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _userService = new UserService(_userRepositoryMock.Object, _authServiceMock.Object);
        }

        [Test]
        public async Task LoginAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "nonexistent", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(loginDto.Username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "user", Password = "wrongpassword" };
            var user = new User { Username = "user", Email = "user@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);
            _authServiceMock.Setup(auth => auth.VerifyPassword(It.IsAny<VerifyPasswordModel>()))
                .Returns(false);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsUserLoginDto()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "user", Password = "password" };
            var user = new User
            {
                Id = "1",
                Username = "user",
                Email = "user@example.com",
                PasswordHash = "hashedPassword",
                PasswordSalt = "salt"
            };

            // Mock the expected token response
            var expectedTokenResponse = new TokenResponseModel
            {
                AccessToken = "jwt_token",
                RefreshToken = "refresh_token",
                RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(30)
            };

            // Mock the user retrieval
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            // Mock the password verification with matching model values
            _authServiceMock.Setup(auth => auth.VerifyPassword(It.Is<VerifyPasswordModel>(model =>
                    model.Password == loginDto.Password &&
                    model.StoredHash == user.PasswordHash &&
                    model.StoredSalt == user.PasswordSalt
            ))).Returns(true);

            // Mock the token generation
            _authServiceMock.Setup(auth => auth.GenerateTokens(user))
                .Returns(expectedTokenResponse);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.AccessToken, Is.EqualTo(expectedTokenResponse.AccessToken));
                Assert.That(result?.RefreshToken, Is.EqualTo(expectedTokenResponse.RefreshToken));
            });
        }

        [Test]
        public async Task RegisterAsync_UsernameTaken_ReturnsErrorResponse()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto { Username = "user", Email = "email@example.com", Password = "password" };
            var existingUser = new User { Username = "user", Email = "email@example.com", PasswordHash = "hash", PasswordSalt = "salt" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterAsync(registrationDto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo("This username is already taken."));
            });
        }

        [Test]
        public async Task RegisterAsync_EmailTaken_ReturnsErrorResponse()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto { Username = "newuser", Email = "email@example.com", Password = "password" };
            var existingUser = new User { Username = "existinguser", Email = "email@example.com", PasswordHash = "hash", PasswordSalt = "salt" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(registrationDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterAsync(registrationDto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo("This email is already registered to a different account."));
            });
        }

        [Test]
        public async Task RegisterAsync_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                Username = "newuser",
                Email = "email@example.com",
                Password = "password"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(registrationDto.Email))
                .ReturnsAsync((User?)null);

            var passwordHashModel = new PasswordHashModel
            {
                Hash = "hashedPassword",
                Salt = "salt"
            };

            _authServiceMock.Setup(auth => auth.HashPassword(registrationDto.Password))
                .Returns(passwordHashModel);

            var newUser = new User
            {
                Id = "1",
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHashModel.Hash,
                PasswordSalt = passwordHashModel.Salt
            };

            _userRepositoryMock.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(newUser));

            // Act
            var result = await _userService.RegisterAsync(registrationDto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data?.Username, Is.EqualTo(newUser.Username));
                Assert.That(result.Data?.Email, Is.EqualTo(newUser.Email));
            });

            _userRepositoryMock.Verify(repo => repo.AddUserAsync(It.Is<User>(u =>
                u.Username == newUser.Username &&
                u.Email == newUser.Email &&
                u.PasswordHash == passwordHashModel.Hash &&
                u.PasswordSalt == passwordHashModel.Salt)), Times.Once);
        }

        #region RefreshToken Tests

        [Test]
        public async Task RefreshTokenAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalid_token" };
            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RefreshTokenAsync(refreshTokenRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsNull()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalid_token" };
            var user = new User
            {
                Username = "user",
                Email = "user@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RefreshToken = "different_token",
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(1)
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.RefreshTokenAsync(refreshTokenRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RefreshTokenAsync_ExpiredRefreshToken_ReturnsNull()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "valid_token" };
            var user = new User
            {
                Username = "user",
                Email = "user@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RefreshToken = "valid_token",
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(-1) // Expired
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.RefreshTokenAsync(refreshTokenRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "valid_token" };
            var user = new User
            {
                Id = "1",
                Username = "user",
                Email = "user@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RefreshToken = "valid_token",
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(1) // Not expired
            };

            var expectedTokenResponse = new TokenResponseModel
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(30)
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(user);

            _authServiceMock.Setup(auth => auth.GenerateTokens(user))
                .Returns(expectedTokenResponse);

            // Act
            var result = await _userService.RefreshTokenAsync(refreshTokenRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.AccessToken, Is.EqualTo(expectedTokenResponse.AccessToken));
                Assert.That(result?.RefreshToken, Is.EqualTo(expectedTokenResponse.RefreshToken));
            });

            // Ensure the user's refresh token and expiration were updated
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                u.RefreshToken == expectedTokenResponse.RefreshToken &&
                u.RefreshTokenExpiration == expectedTokenResponse.RefreshTokenExpirationDate)), Times.Once);
        }

        #endregion
    }
}
