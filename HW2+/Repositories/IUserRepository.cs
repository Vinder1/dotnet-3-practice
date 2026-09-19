using HW2.Models;

namespace HW2.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsWithUsernameAsync(string username);
    Task<bool> ExistsWithEmailAsync(string email);
}