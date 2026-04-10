using Microsoft.AspNetCore.Identity;
using ProjectManagement.DTOs.Common;
using Task = System.Threading.Tasks.Task;
using ProjectManagement.DTOs.Jira;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Entities;
using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IReleaseRepository _releaseRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IJiraService _jiraService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IReleaseRepository releaseRepository,
        IUserRepository userRepository,
        UserManager<User> userManager,
        IJiraService jiraService,
        ILogger<TaskService> logger)
    {
        _taskRepository    = taskRepository;
        _releaseRepository = releaseRepository;
        _userRepository    = userRepository;
        _userManager       = userManager;
        _jiraService       = jiraService;
        _logger            = logger;
    }

    public async Task<PagedResult<TaskResponseDto>> GetAllAsync(int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null)
    {
        var (items, totalCount) = await _taskRepository.GetAllPagedAsync(pageNumber, pageSize, status, assignedToUsername, userRole);
        return new PagedResult<TaskResponseDto>
        {
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<TaskResponseDto>> GetByReleaseAsync(int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null)
    {
        var (items, totalCount) = await _taskRepository.GetByReleaseIdPagedAsync(releaseId, pageNumber, pageSize, status, assignedToUsername, userRole);
        return new PagedResult<TaskResponseDto>
        {
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TaskResponseDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task is null ? null : MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        if (await _releaseRepository.GetByIdAsync(dto.ReleaseId) is null)
            throw new NotFoundException($"Release with id {dto.ReleaseId} not found.");

        var devUser = await _userRepository.GetByIdAsync(dto.AssignedToUserId)
            ?? throw new NotFoundException($"User with id {dto.AssignedToUserId} not found.");
        if (!await _userManager.IsInRoleAsync(devUser, nameof(UserRole.Developer)))
            throw new BadRequestException("AssignedToUserId must refer to a Developer.");

        var qaUser = await _userRepository.GetByIdAsync(dto.AssignedToQAUserId)
            ?? throw new NotFoundException($"QA user with id {dto.AssignedToQAUserId} not found.");
        if (!await _userManager.IsInRoleAsync(qaUser, nameof(UserRole.QA)))
            throw new BadRequestException("AssignedToQAUserId must refer to a QA user.");

        var task = new Entities.Task
        {
            Title = dto.Title,
            ReleaseId = dto.ReleaseId,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedToQAUserId = dto.AssignedToQAUserId,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _taskRepository.AddAsync(task);
        var full = await _taskRepository.GetByIdAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task with id {id} not found.");

        var devUser = await _userRepository.GetByIdAsync(dto.AssignedToUserId)
            ?? throw new NotFoundException($"User with id {dto.AssignedToUserId} not found.");
        if (!await _userManager.IsInRoleAsync(devUser, nameof(UserRole.Developer)))
            throw new BadRequestException("AssignedToUserId must refer to a Developer.");

        var qaUser = await _userRepository.GetByIdAsync(dto.AssignedToQAUserId)
            ?? throw new NotFoundException($"QA user with id {dto.AssignedToQAUserId} not found.");
        if (!await _userManager.IsInRoleAsync(qaUser, nameof(UserRole.QA)))
            throw new BadRequestException("AssignedToQAUserId must refer to a QA user.");

        task.Title = dto.Title;
        task.AssignedToUserId = dto.AssignedToUserId;
        task.AssignedToQAUserId = dto.AssignedToQAUserId;

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> UpdateStatusAsync(
        int id,
        UpdateTaskStatusDto dto,
        int requestingUserId,
        UserRole requestingUserRole)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task with id {id} not found.");

        var devStatuses = new[] { TaskStatus.InProgress, TaskStatus.PRRaised, TaskStatus.Merged };
        var qaStatuses  = new[] { TaskStatus.QAApproved, TaskStatus.Deployed, TaskStatus.Done };

        if (requestingUserRole == UserRole.Developer)
        {
            if (!devStatuses.Contains(dto.NewStatus))
                throw new ForbiddenException("Developers can only advance tasks up to 'Merged'.");
            if (task.AssignedToUserId != requestingUserId)
                throw new ForbiddenException("You are not the assigned developer for this task.");
        }
        else if (requestingUserRole == UserRole.QA)
        {
            if (!qaStatuses.Contains(dto.NewStatus))
                throw new ForbiddenException("QA can only advance tasks from 'Merged' through 'Done'.");
            if (task.AssignedToQAUserId != requestingUserId)
                throw new ForbiddenException("You are not the assigned QA for this task.");
        }

        if (!TaskStatusTransitionService.IsValid(task.Status, dto.NewStatus))
            throw new BadRequestException(
                $"Invalid status transition from '{task.Status}' to '{dto.NewStatus}'. " +
                $"Expected next status: '{TaskStatusTransitionService.GetNextStatus(task.Status)}'.");

        task.Status = dto.NewStatus;
        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> UpdateDevFieldsAsync(int id, UpdateDevFieldsDto dto, int requestingUserId)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task with id {id} not found.");

        if (task.AssignedToUserId != requestingUserId)
            throw new ForbiddenException("You are not assigned to this task.");

        task.PRLink = dto.PRLink;
        task.Remarks = dto.Remarks;

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> UpdateQAFieldsAsync(int id, UpdateQAFieldsDto dto, int requestingUserId)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task with id {id} not found.");

        if (task.AssignedToQAUserId != requestingUserId)
            throw new ForbiddenException("You are not the assigned QA for this task.");

        task.Remarks = dto.Remarks;

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task with id {id} not found.");

        await _taskRepository.DeleteAsync(task);
    }

    // ── Jira integration ──────────────────────────────────────────────────────

    public async Task<TaskResponseDto> LinkJiraIssueAsync(int taskId, string issueKey)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new NotFoundException($"Task with id {taskId} not found.");

        // Validate the issue exists in Jira before linking
        await _jiraService.GetIssueAsync(issueKey);

        task.JiraIssueKey = issueKey;
        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(taskId);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> UnlinkJiraIssueAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new NotFoundException($"Task with id {taskId} not found.");

        task.JiraIssueKey = null;
        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(taskId);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> CreateJiraIssueForTaskAsync(int taskId, CreateJiraIssueDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new NotFoundException($"Task with id {taskId} not found.");

        if (!string.IsNullOrEmpty(task.JiraIssueKey))
            throw new ConflictException($"Task is already linked to Jira issue {task.JiraIssueKey}.");

        if (string.IsNullOrWhiteSpace(dto.ProjectKey))
            throw new BadRequestException("ProjectKey is required to create a Jira issue.");

        var issueKey = await _jiraService.CreateIssueAsync(
            dto.ProjectKey,
            task.Title,
            dto.IssueType ?? "Task",
            dto.Description);

        task.JiraIssueKey = issueKey;
        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(taskId);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> ImportFromJiraAsync(JiraImportDto dto)
    {
        if (await _releaseRepository.GetByIdAsync(dto.ReleaseId) is null)
            throw new NotFoundException($"Release with id {dto.ReleaseId} not found.");

        var devUser = await _userRepository.GetByIdAsync(dto.AssignedToUserId)
            ?? throw new NotFoundException($"User with id {dto.AssignedToUserId} not found.");
        if (!await _userManager.IsInRoleAsync(devUser, nameof(UserRole.Developer)))
            throw new BadRequestException("AssignedToUserId must refer to a Developer.");

        var qaUser = await _userRepository.GetByIdAsync(dto.AssignedToQAUserId)
            ?? throw new NotFoundException($"QA user with id {dto.AssignedToQAUserId} not found.");
        if (!await _userManager.IsInRoleAsync(qaUser, nameof(UserRole.QA)))
            throw new BadRequestException("AssignedToQAUserId must refer to a QA user.");

        var jiraIssue = await _jiraService.GetIssueAsync(dto.JiraIssueKey);

        var task = new Entities.Task
        {
            Title               = jiraIssue.Summary,
            JiraIssueKey        = dto.JiraIssueKey,
            ReleaseId           = dto.ReleaseId,
            AssignedToUserId    = dto.AssignedToUserId,
            AssignedToQAUserId  = dto.AssignedToQAUserId,
            Status              = TaskStatus.Pending,
            CreatedAt           = DateTime.UtcNow
        };

        var created = await _taskRepository.AddAsync(task);
        var full    = await _taskRepository.GetByIdAsync(created.Id);
        return MapToDto(full!);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static TaskResponseDto MapToDto(Entities.Task task) => new()
    {
        Id                   = task.Id,
        Title                = task.Title,
        ReleaseId            = task.ReleaseId,
        ReleaseTitle         = task.Release?.Title ?? string.Empty,
        AssignedToUserId     = task.AssignedToUserId,
        AssignedToUsername   = task.AssignedToUser?.UserName ?? string.Empty,
        AssignedToQAUserId   = task.AssignedToQAUserId,
        AssignedToQAUsername = task.AssignedToQAUser?.UserName,
        PRLink               = task.PRLink,
        Remarks              = task.Remarks,
        JiraIssueKey         = task.JiraIssueKey,
        Status               = task.Status,
        CreatedAt            = task.CreatedAt
    };
}
