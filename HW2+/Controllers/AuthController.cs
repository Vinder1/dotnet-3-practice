using HW2.DTOs;
using HW2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService userService, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _userService.LoginAsync(request));
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "Login request denied");
            return Unauthorized("Invalid username/email or password.");
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var registerResponse = await _userService.RegisterAsync(request);
            return Created($"/api/users/{registerResponse.User.Id}", registerResponse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration request conflicts with existing user");
            return Conflict(ex.Message);
        }
    }
}