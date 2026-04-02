using ProjectManagement.DTOs.Task;

namespace ProjectManagement.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<Entities.Task?> GetByIdAsync(int id);
    Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetByReleaseIdPagedAsync(int releaseId, int pageNumber, int pageSize);
    Task<Entities.Task> AddAsync(Entities.Task task);
    Task UpdateAsync(Entities.Task task);
    Task DeleteAsync(Entities.Task task);
}
