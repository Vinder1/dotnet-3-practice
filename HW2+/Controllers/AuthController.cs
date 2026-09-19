using HW2.DTOs;
using HW2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _userService.LoginAsync(request));
        }
        catch (UnauthorizedAccessException)
        {
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
            return Conflict(ex.Message);
        }
    }
}