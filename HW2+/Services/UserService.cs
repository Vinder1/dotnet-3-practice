using HW2.DTOs;
using HW2.Models;
using HW2.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HW2.Services;

public class UserService(
    IUserRepository repository,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher) : IUserService
{
    private readonly IUserRepository _repository = repository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _repository.ExistsWithUsernameAsync(request.Username))
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        if (await _repository.ExistsWithEmailAsync(request.Email))
        {
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
        return ToRegisterResponse(created);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        return ToLoginResponse(user);
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(FilterUsersRequest filter)
    {
        var users = await _repository.GetAllAsync(
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.UpdatedFrom,
            filter.UpdatedTo);
    
        return users.Select(ToUserResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");
        return ToUserResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (user.Username == request.Username ||
                await _repository.ExistsWithUsernameAsync(request.Username))
            {
                throw new InvalidOperationException("A user with this username already exists.");
            }

            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (user.Email == request.Email ||
                await _repository.ExistsWithEmailAsync(request.Email))
            {
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
        return ToUserResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
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