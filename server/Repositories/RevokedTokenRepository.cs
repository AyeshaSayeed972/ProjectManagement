using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.Repositories;

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly AppDbContext _context;

    public RevokedTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsRevokedAsync(string jti)
        => await _context.RevokedTokens.AnyAsync(t => t.Jti == jti);

    public async Task RevokeAsync(string jti, DateTime expiresAt)
    {
        _context.RevokedTokens.Add(new RevokedToken { Jti = jti, ExpiresAt = expiresAt });
        await _context.SaveChangesAsync();
    }
}
