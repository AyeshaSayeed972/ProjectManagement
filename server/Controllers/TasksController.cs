using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Jira;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using ProjectManagement.Services.Interfaces;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

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

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery pagination,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] string? assignedToUsername = null,
        [FromQuery] string? userRole = null)
    {
        var result = await _taskService.GetAllAsync(pagination.PageNumber, pagination.PageSize, status, assignedToUsername, userRole);
        return Ok(result);
    }

    [HttpGet("release/{releaseId:int}")]
    public async Task<IActionResult> GetByRelease(
        int releaseId,
        [FromQuery] PaginationQuery pagination,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] string? assignedToUsername = null,
        [FromQuery] string? userRole = null)
    {
        var result = await _taskService.GetByReleaseAsync(releaseId, pagination.PageNumber, pagination.PageSize, status, assignedToUsername, userRole);
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

    // ── Jira endpoints ────────────────────────────────────────────────────────

    [HttpPatch("{id:int}/jira/link")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> LinkJiraIssue(int id, [FromBody] LinkJiraIssueDto dto)
    {
        var updated = await _taskService.LinkJiraIssueAsync(id, dto.JiraIssueKey);
        return Ok(updated);
    }

    [HttpDelete("{id:int}/jira/link")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> UnlinkJiraIssue(int id)
    {
        var updated = await _taskService.UnlinkJiraIssueAsync(id);
        return Ok(updated);
    }

    [HttpPost("{id:int}/jira/create-issue")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> CreateJiraIssueForTask(int id, [FromBody] CreateJiraIssueDto dto)
    {
        var updated = await _taskService.CreateJiraIssueForTaskAsync(id, dto);
        return Ok(updated);
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id))
            throw new ForbiddenException("Invalid user identity claim.");
        return id;
    }

    private UserRole GetCurrentUserRole()
    {
        var value = User.FindFirstValue(ClaimTypes.Role);
        if (!Enum.TryParse<UserRole>(value, out var role))
            throw new ForbiddenException("Invalid role claim.");
        return role;
    }
}
