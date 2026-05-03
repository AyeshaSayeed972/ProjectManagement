using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.BLL;
using ProjectManagement.DAL;

namespace ProjectManagement.Presentation;

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
