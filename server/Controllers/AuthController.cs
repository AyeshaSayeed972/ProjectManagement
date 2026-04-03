using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Auth;
using ProjectManagement.Exceptions;
using ProjectManagement.Services.Interfaces;
using System.Security.Claims;

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
        var result = await _authService.LoginAsync(dto);
        SetAuthCookies(result.AccessToken, result.RefreshToken);
        return Ok(new { username = result.Username, role = result.Role.ToString() });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            throw new ForbiddenException("Refresh token is missing.");

        var result = await _authService.RefreshAsync(refreshToken);
        SetAuthCookies(result.AccessToken, result.RefreshToken);
        return Ok(new { username = result.Username, role = result.Role.ToString() });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var accessToken  = Request.Cookies["access_token"]  ?? string.Empty;
        var refreshToken = Request.Cookies["refresh_token"] ?? string.Empty;

        await _authService.LogoutAsync(accessToken, refreshToken);

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/auth" });

        return Ok(new { success = true, message = "Logged out successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role     = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddDays(7),
            Path     = "/api/auth"   // only sent to the auth endpoints
        });
    }
}
