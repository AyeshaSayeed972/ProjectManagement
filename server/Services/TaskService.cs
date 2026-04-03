using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Task;
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

    public TaskService(
        ITaskRepository taskRepository,
        IReleaseRepository releaseRepository,
        IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _releaseRepository = releaseRepository;
        _userRepository = userRepository;
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
        if (devUser.Role != UserRole.Developer)
            throw new BadRequestException("AssignedToUserId must refer to a Developer.");

        var qaUser = await _userRepository.GetByIdAsync(dto.AssignedToQAUserId)
            ?? throw new NotFoundException($"QA user with id {dto.AssignedToQAUserId} not found.");
        if (qaUser.Role != UserRole.QA)
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
        if (devUser.Role != UserRole.Developer)
            throw new BadRequestException("AssignedToUserId must refer to a Developer.");

        var qaUser = await _userRepository.GetByIdAsync(dto.AssignedToQAUserId)
            ?? throw new NotFoundException($"QA user with id {dto.AssignedToQAUserId} not found.");
        if (qaUser.Role != UserRole.QA)
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

    private static TaskResponseDto MapToDto(Entities.Task task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        ReleaseId = task.ReleaseId,
        ReleaseTitle = task.Release?.Title ?? string.Empty,
        AssignedToUserId = task.AssignedToUserId,
        AssignedToUsername = task.AssignedToUser?.Username ?? string.Empty,
        AssignedToQAUserId = task.AssignedToQAUserId,
        AssignedToQAUsername = task.AssignedToQAUser?.Username,
        PRLink = task.PRLink,
        Remarks = task.Remarks,
        Status = task.Status,
        CreatedAt = task.CreatedAt
    };
}
