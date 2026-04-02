namespace ProjectManagement.Repositories.Interfaces;

public interface IRevokedTokenRepository
{
    Task<bool> IsRevokedAsync(string jti);
    Task RevokeAsync(string jti, DateTime expiresAt);
}
