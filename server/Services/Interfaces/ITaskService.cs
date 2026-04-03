using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Enums;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskResponseDto>> GetAllAsync(int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    Task<PagedResult<TaskResponseDto>> GetByReleaseAsync(int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    Task<TaskResponseDto?> GetByIdAsync(int id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto);
    Task<TaskResponseDto> UpdateStatusAsync(int id, UpdateTaskStatusDto dto, int requestingUserId, UserRole requestingUserRole);
    Task<TaskResponseDto> UpdateDevFieldsAsync(int id, UpdateDevFieldsDto dto, int requestingUserId);
    Task<TaskResponseDto> UpdateQAFieldsAsync(int id, UpdateQAFieldsDto dto, int requestingUserId);
    Task DeleteAsync(int id);
}
