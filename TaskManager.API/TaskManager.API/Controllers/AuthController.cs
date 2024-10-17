using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models.Common;
using TaskManager.Models.User;
using TaskManager.Services.Interfaces;

namespace TaskManager.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IUserService userService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto model)
        {
            var registrationResponse = await userService.RegisterAsync(model);

            if (!registrationResponse.Success)
            {
                return BadRequest(registrationResponse.ErrorMessage);
            }

            return Ok(registrationResponse.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var authResponse = await userService.LoginAsync(model);

            if (authResponse == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(authResponse);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var authResponse = await userService.RefreshTokenAsync(model);

            if (authResponse == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(authResponse);
        }
    }
}