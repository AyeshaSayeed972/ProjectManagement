using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Enums;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("release/{releaseId:int}")]
    public async Task<IActionResult> GetByRelease(int releaseId, [FromQuery] PaginationQuery pagination)
    {
        var result = await _taskService.GetByReleaseAsync(releaseId, pagination.PageNumber, pagination.PageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var created = await _taskService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var updated = await _taskService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "PM,Developer,QA")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var updated = await _taskService.UpdateStatusAsync(id, dto, userId, userRole);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/dev-fields")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> UpdateDevFields(int id, [FromBody] UpdateDevFieldsDto dto)
    {
        var userId = GetCurrentUserId();
        var updated = await _taskService.UpdateDevFieldsAsync(id, dto, userId);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/qa-fields")]
    [Authorize(Roles = "QA")]
    public async Task<IActionResult> UpdateQAFields(int id, [FromBody] UpdateQAFieldsDto dto)
    {
        var userId = GetCurrentUserId();
        var updated = await _taskService.UpdateQAFieldsAsync(id, dto, userId);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private UserRole GetCurrentUserRole()
        => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
}
