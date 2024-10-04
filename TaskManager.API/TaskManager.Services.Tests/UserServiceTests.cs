using Moq;
using TaskManager.DataAccess.Repository;
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
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "user", Password = "wrongpassword" };
            var user = new User { Username = "user", Email = "user@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);
            _authServiceMock.Setup(auth => auth.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                .Returns(false);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "user", Password = "password" };
            var user = new User { Username = "user", Email = "user@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var expectedToken = "jwt_token";

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);
            _authServiceMock.Setup(auth => auth.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                .Returns(true);
            _authServiceMock.Setup(auth => auth.GenerateJwtToken(user))
                .Returns(expectedToken);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.AreEqual(expectedToken, result);
        }

        [Test]
        public void RegisterAsync_UsernameTaken_ThrowsInvalidOperationException()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto { Username = "user", Email = "email@example.com", Password = "password" };
            var existingUser = new User { Username = "user", Email = "email@example.com", PasswordHash = "hash", PasswordSalt = "salt" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterAsync(registrationDto));
            Assert.AreEqual("This username is already taken.", ex.Message);
        }

        [Test]
        public void RegisterAsync_EmailTaken_ThrowsInvalidOperationException()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto { Username = "newuser", Email = "email@example.com", Password = "password" };
            var existingUser = new User { Username = "existinguser", Email = "email@example.com", PasswordHash = "hash", PasswordSalt = "salt" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(registrationDto.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterAsync(registrationDto));
            Assert.AreEqual("This email is already registered to a different account.", ex.Message);
        }

        [Test]
        public async Task RegisterAsync_ValidData_CreatesUser()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto { Username = "newuser", Email = "email@example.com", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(registrationDto.Username))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(registrationDto.Email))
                .ReturnsAsync((User)null);

            var (passwordHash, passwordSalt) = ("hashedPassword", "salt");
            _authServiceMock.Setup(auth => auth.HashPassword(registrationDto.Password))
                .Returns((passwordHash, passwordSalt));

            var newUser = new User
            {
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _userRepositoryMock.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult<User>(null));

            // Act
            var result = await _userService.RegisterAsync(registrationDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(newUser.Username, result.Username);
            Assert.AreEqual(newUser.Email, result.Email);
            _userRepositoryMock.Verify(repo => repo.AddUserAsync(It.Is<User>(u => u.Username == newUser.Username)), Times.Once);
        }
    }
}