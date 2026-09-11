using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HW2.DTOs;
using HW2.Models;
using HW2.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HW2.Services;

public class UserService(
    IUserRepository repository,
    IConfiguration configuration,
    IPasswordHasher<User>  passwordHasher) : IUserService
{
    private readonly IUserRepository _repository = repository;
    private readonly IConfiguration _configuration = configuration;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
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
            Email = request.Email
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var created = await _repository.AddAsync(user);
        return ToResponse(created);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, request.Password, user.PasswordHash) != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        return new LoginResponse
        {
            Token = GenerateToken(user),
            ExpiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
            User = ToResponse(user)
        };
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");
        return ToResponse(user);
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

        var updated = await _repository.UpdateAsync(user);
        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not set.")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}