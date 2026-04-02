using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Repositories.Interfaces;

namespace ProjectManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    // Translates to SELECT 1 ... LIMIT 1 — used for FK existence checks before creates/updates.
    public async Task<bool> ExistsAsync(int id)
        => await _context.Users.AnyAsync(u => u.Id == id);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Users.CountAsync();
        var items = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}
