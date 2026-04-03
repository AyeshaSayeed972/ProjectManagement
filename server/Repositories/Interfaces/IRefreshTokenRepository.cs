using ProjectManagement.Entities;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
}
