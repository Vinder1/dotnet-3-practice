using HW2.DTOs;

namespace HW2.Services;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
}