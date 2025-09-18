using Api.Application.Auth.Dtos;
using Api.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("/")]
        public IActionResult Test()
        {
            return Ok("Api is running.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var res = await _authService.RegisterAsync(model);

            if (res.Succeeded)
            {
                return Ok(new { Message = "User registered successfully." });
            }
            return BadRequest(res.Errors);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
           var res = await _authService.LoginAsync(loginDto);
           return Ok(res);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refresh)
        {
            var res = await _authService.RefreshTokenAsync(refresh);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var res = await _authService.GetMyInfo();
            return Ok(res);

        }
    }
}
