using ProjectManagement.DTOs.Auth;

namespace ProjectManagement.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
}
