using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.Models.Common;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.API.Tests.Controllers;

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

        var responseDto = new ResponseDto<UserRegistrationResponse>
        {
            Success = true,
            Data = new UserRegistrationResponse
            {
                Username = "test-user",
                Email = "test@example.com"
            }
        };

        // Mock the service to return a successful registration
        _userServiceMock.Setup(s => s.RegisterAsync(registrationDto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _authController.Register(registrationDto);

        // Assert
        Assert.Multiple(() =>
        {
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult");
            var returnedResponse = okResult?.Value as UserRegistrationResponse;

            Assert.That(returnedResponse, Is.Not.Null, "Expected returnedResponse to be not null");
            Assert.That(returnedResponse?.Username, Is.EqualTo("test-user"));
            Assert.That(returnedResponse?.Email, Is.EqualTo("test@example.com"));
        });
    }

    [Test]
    public async Task Register_ExistingUsername_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto
        {
            Username = "existing-user",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var responseDto = new ResponseDto<UserRegistrationResponse>
        {
            Success = false,
            ErrorMessage = "This username is already taken."
        };

        // Mock the service to return a failed registration due to existing username
        _userServiceMock.Setup(s => s.RegisterAsync(registrationDto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _authController.Register(registrationDto);

        // Assert
        Assert.Multiple(() =>
        {
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "Expected BadRequestObjectResult");

            Assert.That(badRequestResult?.Value, Is.EqualTo("This username is already taken."));
        });
    }

    [Test]
    public async Task Register_ExistingEmail_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto
        {
            Username = "new-user",
            Email = "existing-email@example.com",
            Password = "Password123!"
        };

        var responseDto = new ResponseDto<UserRegistrationResponse>
        {
            Success = false,
            ErrorMessage = "This email is already registered to a different account."
        };

        // Mock the service to return a failed registration due to existing email
        _userServiceMock.Setup(s => s.RegisterAsync(registrationDto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _authController.Register(registrationDto);

        // Assert
        Assert.Multiple(() =>
        {
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "Expected BadRequestObjectResult");

            Assert.That(badRequestResult?.Value, Is.EqualTo("This email is already registered to a different account."));
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

        var userLoginResponse = new UserLoginResponse
        {
            AccessToken = "mocked_jwt_token",
            RefreshToken = "mocked_refresh_token"
        };

        // Mock the service to return a UserLoginResponse
        _userServiceMock.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(userLoginResponse);  // Directly return the response

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult");

        // Ensure the value is not null
        Assert.That(okResult.Value, Is.Not.Null, "Expected Value to be not null");

        // Cast the value to UserLoginResponse to check for the token
        var tokenResponse = okResult.Value as UserLoginResponse;

        // Check for the AccessToken property
        Assert.Multiple(() =>
        {
            Assert.That(tokenResponse, Is.Not.Null, "Expected tokenResponse to be not null");
            Assert.That(tokenResponse?.AccessToken, Is.EqualTo("mocked_jwt_token"),
                "Expected AccessToken to match the mocked token.");
            Assert.That(tokenResponse?.RefreshToken, Is.EqualTo("mocked_refresh_token"),
                "Expected RefreshToken to match the mocked refresh token.");
        });
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
            .ReturnsAsync((UserLoginResponse?)null);  // Directly return null

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null, "Expected UnauthorizedObjectResult");
        Assert.That(unauthorizedResult.Value, Is.EqualTo("Invalid credentials")); // Check for the error message
    }

    #endregion

    #region RefreshToken Tests

    [Test]
    public async Task RefreshToken_ShouldReturnOk_WhenTokenIsValid()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var controller = new AuthController(mockUserService.Object);

        var validRequest = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };
        var validResponse = new UserLoginResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token"
        };

        mockUserService.Setup(x => x.RefreshTokenAsync(validRequest))
            .ReturnsAsync(validResponse);

        // Act
        var result = await controller.RefreshToken(validRequest);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult");
        Assert.That(okResult.Value, Is.EqualTo(validResponse));
        mockUserService.Verify(x => x.RefreshTokenAsync(validRequest), Times.Once);
    }

    [Test]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var controller = new AuthController(mockUserService.Object);

        var invalidRequest = new RefreshTokenRequest { RefreshToken = "invalid-refresh-token" };

        mockUserService.Setup(x => x.RefreshTokenAsync(invalidRequest))
            .ReturnsAsync((UserLoginResponse?)null);

        // Act
        var result = await controller.RefreshToken(invalidRequest);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null, "Expected UnauthorizedObjectResult");
        mockUserService.Verify(x => x.RefreshTokenAsync(invalidRequest), Times.Once);
    }

    #endregion
}