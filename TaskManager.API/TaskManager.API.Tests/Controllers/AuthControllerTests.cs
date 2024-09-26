using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.Models.User;
using TaskManager.Services;

namespace TaskManager.API.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private AuthController _authController;

        [SetUp]
        public void SetUp()
        {
            // Initialize the mock service and controller before each test
            _userServiceMock = new Mock<IUserService>();
            _authController = new AuthController(_userServiceMock.Object);
        }

        #region Register Tests

        [Test]
        public async Task Register_ValidUser_ReturnsOkResultWithUserDetails()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                Username = "test-user",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var createdUser = new User
            {
                Username = "test-user",
                Email = "test@example.com"
            };

            // Mock the service to return the created user
            _userServiceMock.Setup(s => s.RegisterAsync(registrationDto))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _authController.Register(registrationDto);

            // Assert
            Assert.Multiple(() =>
            {
                var okResult = result as OkObjectResult;
                Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult");
                var returnedUser = okResult?.Value as UserRegistrationResponse;

                Assert.That(returnedUser, Is.Not.Null, "Expected returnedUser to be not null");
                Assert.That(returnedUser?.Username, Is.EqualTo("test-user"));
                Assert.That(returnedUser?.Email, Is.EqualTo("test@example.com"));
            });
        }

        #endregion

        #region Login Tests
        [Test]
        public async Task Login_ValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Username = "test-user",
                Password = "Password123!"
            };

            var token = "mocked_jwt_token";

            // Mock the service to return a JWT token
            _userServiceMock.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(token);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult");

            // Ensure the value is not null
            Assert.That(okResult.Value, Is.Not.Null, "Expected Value to be not null");

            // Cast the value to an anonymous object to check for the token
            var tokenResponse = okResult.Value as UserLoginResponse;

            // Check for the Token property
            Assert.That(tokenResponse, Is.Not.Null, "Expected tokenResponse to be not null");
            Assert.That(tokenResponse.Token, Is.EqualTo(token), "Expected Token to match the mocked token.");
        }




        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Username = "test-user",
                Password = "wrong-password"
            };

            // Mock the service to return null for invalid credentials
            _userServiceMock.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null, "Expected UnauthorizedObjectResult");
            Assert.That(unauthorizedResult.Value, Is.EqualTo("Invalid credentials"));
        }

        #endregion
    }
}
