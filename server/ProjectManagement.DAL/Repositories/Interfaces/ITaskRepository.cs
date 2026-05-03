using TaskStatus = ProjectManagement.DAL.TaskStatus;

namespace ProjectManagement.DAL;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<Task?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetByReleaseIdPagedAsync(int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    System.Threading.Tasks.Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    System.Threading.Tasks.Task<Task> AddAsync(Task task);
    System.Threading.Tasks.Task UpdateAsync(Task task);
    System.Threading.Tasks.Task DeleteAsync(Task task);
}
