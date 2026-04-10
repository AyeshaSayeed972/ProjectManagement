using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Enums;
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

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPagedAsync(int pageNumber, int pageSize, UserRole? role)
    {
        IQueryable<User> query;

        if (role.HasValue)
        {
            var roleName = role.Value.ToString();
            query = _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r.Name })
                .Where(x => x.Name == roleName)
                .Select(x => x.u)
                .AsNoTracking();
        }
        else
        {
            query = _context.Users.AsNoTracking();
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}
