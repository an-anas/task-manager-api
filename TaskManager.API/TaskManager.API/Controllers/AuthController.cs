using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var user = await userService.RegisterAsync(model);
            return Ok(new UserRegistrationResponse { Username = user.Username, Email = user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var token = await userService.LoginAsync(model);

            if (token == null)
                return Unauthorized("Invalid credentials");

            return Ok(new UserLoginResponse { Token = token });
        }
    }
}
