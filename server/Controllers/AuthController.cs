using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Auth;
using ProjectManagement.Entities;
using ProjectManagement.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly SignInManager<User> _signInManager;

    public AuthController(IAuthService authService, SignInManager<User> signInManager)
    {
        _authService   = authService;
        _signInManager = signInManager;
    }

    [HttpGet("antiforgery")]
    [AllowAnonymous]
    public IActionResult GetAntiforgery([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        });
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto,
        [FromServices] IAntiforgery antiforgery)
    {
        var result = await _authService.LoginAsync(dto);

        // Regenerate antiforgery token bound to the newly authenticated user,
        // replacing the anonymous token the client held before login.
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        });

        return Ok(new { username = result.Username, role = result.Role.ToString() });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
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
}
