using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Entities;

namespace ProjectManagement.Auth;

public class JwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(User user)
    {
        var jti = Guid.NewGuid().ToString();

        var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, issuedAt, ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Returns (rawToken, tokenHash). rawToken goes to the client; tokenHash is stored in DB.
    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var rawToken = Convert.ToBase64String(bytes);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        var tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return (rawToken, tokenHash);
    }

    public (string Jti, DateTime ExpiresAt) ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var jti = jwt.Id;
        var expiresAt = jwt.ValidTo;
        return (jti, expiresAt);
    }
}
