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

    public async Task<PagedResult<TaskResponseDto>> GetByReleaseAsync(int releaseId, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _taskRepository.GetByReleaseIdPagedAsync(releaseId, pageNumber, pageSize);
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

        if (!await _userRepository.ExistsAsync(dto.AssignedToUserId))
            throw new NotFoundException($"User with id {dto.AssignedToUserId} not found.");

        var task = new Entities.Task
        {
            Title = dto.Title,
            ReleaseId = dto.ReleaseId,
            AssignedToUserId = dto.AssignedToUserId,
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

        if (!await _userRepository.ExistsAsync(dto.AssignedToUserId))
            throw new NotFoundException($"User with id {dto.AssignedToUserId} not found.");

        task.Title = dto.Title;
        task.AssignedToUserId = dto.AssignedToUserId;

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

        if (requestingUserRole != UserRole.PM && task.AssignedToUserId != requestingUserId)
            throw new ForbiddenException("You are not assigned to this task.");

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

        if (task.AssignedToUserId != requestingUserId)
            throw new ForbiddenException("You are not assigned to this task.");

        task.ApproverName = dto.ApproverName;
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
        AssignedToUserId = task.AssignedToUserId,
        AssignedToUsername = task.AssignedToUser?.Username ?? string.Empty,
        PRLink = task.PRLink,
        ApproverName = task.ApproverName,
        Remarks = task.Remarks,
        Status = task.Status,
        CreatedAt = task.CreatedAt
    };
}
