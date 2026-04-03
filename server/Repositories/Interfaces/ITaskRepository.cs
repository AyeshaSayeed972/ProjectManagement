using ProjectManagement.DTOs.Task;
using ProjectManagement.Enums;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<Entities.Task?> GetByIdAsync(int id);
    Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetByReleaseIdPagedAsync(int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    Task<Entities.Task> AddAsync(Entities.Task task);
    Task UpdateAsync(Entities.Task task);
    Task DeleteAsync(Entities.Task task);
}
