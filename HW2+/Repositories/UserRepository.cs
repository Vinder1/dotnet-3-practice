using HW2.Models;
using Microsoft.EntityFrameworkCore;

namespace HW2.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyCollection<User>> GetAllAsync(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        DateTime? updatedFrom = null,
        DateTime? updatedTo = null)
    {
        IQueryable<User> query = _context.Users;

        if (createdFrom is not null)
        {
            query = query.Where(u => u.CreatedAt >= createdFrom);
        }

        if (createdTo is not null)
        {
            query = query.Where(u => u.CreatedAt <= createdTo);
        }

        if (updatedFrom is not null)
        {
            query = query.Where(u => u.UpdatedAt >= updatedFrom);
        }

        if (updatedTo is not null)
        {
            query = query.Where(u => u.UpdatedAt <= updatedTo);
        }

        try
        {
            return await query
                .OrderBy(u => u.Id)
                .ToListAsync();
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid time format.");
        }
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsWithUsernameAsync(string username)
    {
        return _context.Users.AnyAsync(u => u.Username == username);
    }

    public Task<bool> ExistsWithEmailAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }
}