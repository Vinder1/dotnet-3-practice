using HW2.DTOs;
using HW2.Models;
using HW2.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HW2.Services;

public class UserService(
    IUserRepository repository,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher,
    ILogger<UserService> logger) : IUserService
{
    private readonly IUserRepository _repository = repository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly ILogger<UserService> _logger = logger;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation(
            "Registration attempt for username {Username}", request.Username);

        if (await _repository.ExistsWithUsernameAsync(request.Username))
        {
            _logger.LogWarning(
                "Registration rejected: username {Username} is already taken", request.Username);
            throw new InvalidOperationException("A user with this username already exists.");
        }

        if (await _repository.ExistsWithEmailAsync(request.Email))
        {
            _logger.LogWarning(
                "Registration rejected: email {Email} is already taken", request.Email);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var created = await _repository.AddAsync(user);

        _logger.LogInformation(
            "User registered with id {UserId} and username {Username}", created.Id, created.Username);

        return ToRegisterResponse(created);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Success)
        {
            _logger.LogWarning(
                "Failed login attempt for {UsernameOrEmail}", request.UsernameOrEmail);
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        _logger.LogInformation(
            "User {UserId} logged in successfully", user.Id);

        return ToLoginResponse(user);
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(FilterUsersRequest filter)
    {
        _logger.LogInformation(
            "Fetching users with filter: created from {CreatedFrom}, created to {CreatedTo}, updated from {UpdatedFrom}, updated to {UpdatedTo}",
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.UpdatedFrom,
            filter.UpdatedTo);

        var users = await _repository.GetAllAsync(
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.UpdatedFrom,
            filter.UpdatedTo);

        _logger.LogInformation("Fetched {UserCount} users", users.Count);

        return users.Select(ToUserResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new KeyNotFoundException("User not found.");
        }

        _logger.LogInformation("User {UserId} fetched", id);

        return ToUserResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found, update skipped", id);
            throw new KeyNotFoundException("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (user.Username == request.Username ||
                await _repository.ExistsWithUsernameAsync(request.Username))
            {
                _logger.LogWarning(
                    "Update of user {UserId} rejected: username {Username} is already taken", id, request.Username);
                throw new InvalidOperationException("A user with this username already exists.");
            }

            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (user.Email == request.Email ||
                await _repository.ExistsWithEmailAsync(request.Email))
            {
                _logger.LogWarning(
                    "Update of user {UserId} rejected: email {Email} is already taken", id, request.Email);
                throw new InvalidOperationException("A user with this email already exists.");
            }

            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} updated. Password changed: {PasswordChanged}",
            id, !string.IsNullOrWhiteSpace(request.Password));

        return ToUserResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("User {UserId} deleted", id);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found, delete skipped", id);
        }

        return deleted;
    }

    private RegisterResponse ToRegisterResponse(User user)
    {
        var token = _tokenService.Generate(user);
        return new RegisterResponse
        {
            Token = token.Token,
            ExpiresInMinutes = token.ExpiresInMinutes,
            User = ToUserResponse(user)
        };
    }

    private LoginResponse ToLoginResponse(User user)
    {
        var token = _tokenService.Generate(user);
        return new LoginResponse
        {
            Token = token.Token,
            ExpiresInMinutes = token.ExpiresInMinutes,
            User = ToUserResponse(user)
        };
    }

    private static UserResponse ToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}