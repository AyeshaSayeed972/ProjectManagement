using System.Security.Cryptography;
using System.Text;
using ProjectManagement.Auth;
using ProjectManagement.DTOs.Auth;
using ProjectManagement.Entities;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRevokedTokenRepository _revokedTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IRevokedTokenRepository revokedTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        JwtTokenService jwtTokenService,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _revokedTokenRepository = revokedTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new ForbiddenException("Invalid username or password.");

        return await IssueTokenPairAsync(user);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (stored is null || !stored.IsActive)
            throw new ForbiddenException("Refresh token is invalid or expired.");

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(stored);

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user is null)
            throw new ForbiddenException("Refresh token is invalid or expired.");

        return await IssueTokenPairAsync(user);
    }

    public async Task LogoutAsync(string accessToken, string refreshToken)
    {
        var (jti, expiresAt) = _jwtTokenService.ParseToken(accessToken);
        await _revokedTokenRepository.RevokeAsync(jti, expiresAt);

        var tokenHash = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);
        }
    }

    private async Task<AuthResponseDto> IssueTokenPairAsync(User user)
    {
        var accessToken = _jwtTokenService.GenerateToken(user);
        var (rawRefreshToken, tokenHash) = _jwtTokenService.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            TokenHash = tokenHash,
            UserId    = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        });

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = rawRefreshToken,
            Username     = user.Username,
            Role         = user.Role
        };
    }

    private static string HashToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
