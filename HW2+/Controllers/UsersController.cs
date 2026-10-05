using HW2.DTOs;
using HW2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FilterUsersRequest filter)
    {
        return Ok(await _userService.GetUsersAsync(filter));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            return Ok(await _userService.GetByIdAsync(id));
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(exception, "User {UserId} not found", id);
            return NotFound();
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            return Ok(await _userService.UpdateAsync(id, request));
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(exception, "User {UserId} not found, update rejected", id);
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Update of user {UserId} conflicts with existing user", id);
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}