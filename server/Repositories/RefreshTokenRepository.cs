using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.Repositories;

public class RefreshTokenRepository(AppDbContext _context) : IRefreshTokenRepository
{

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        => await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

    public async Task AddAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
}
