using ProjectManagement.DTOs.Auth;

namespace ProjectManagement.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(string accessToken, string refreshToken);
}
