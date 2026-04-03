using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Common;
using ProjectManagement.Enums;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PM")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination, [FromQuery] UserRole? role)
    {
        var result = await _userService.GetAllAsync(pagination.PageNumber, pagination.PageSize, role);
        return Ok(result);
    }
}
