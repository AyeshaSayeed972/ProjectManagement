using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Auth;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var response = await _authService.RefreshAsync(dto.RefreshToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        await _authService.LogoutAsync(accessToken, dto.RefreshToken);
        return Ok(new { success = true, message = "Logged out successfully." });
    }
}
