using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;
using TaskStatus = ProjectManagement.DAL.TaskStatus;

namespace ProjectManagement.BLL;

public interface ITaskService
{
    System.Threading.Tasks.Task<PagedResult<TaskResponseDto>> GetAllAsync(int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    System.Threading.Tasks.Task<PagedResult<TaskResponseDto>> GetByReleaseAsync(int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null);
    System.Threading.Tasks.Task<TaskResponseDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    System.Threading.Tasks.Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto);
    System.Threading.Tasks.Task<TaskResponseDto> UpdateStatusAsync(int id, UpdateTaskStatusDto dto, int requestingUserId, UserRole requestingUserRole);
    System.Threading.Tasks.Task<TaskResponseDto> UpdateDevFieldsAsync(int id, UpdateDevFieldsDto dto, int requestingUserId);
    System.Threading.Tasks.Task<TaskResponseDto> UpdateQAFieldsAsync(int id, UpdateQAFieldsDto dto, int requestingUserId);
    Task DeleteAsync(int id);

    // Jira integration
    System.Threading.Tasks.Task<TaskResponseDto> LinkJiraIssueAsync(int taskId, string issueKey);
    System.Threading.Tasks.Task<TaskResponseDto> UnlinkJiraIssueAsync(int taskId);
    System.Threading.Tasks.Task<TaskResponseDto> CreateJiraIssueForTaskAsync(int taskId, CreateJiraIssueDto dto);
    System.Threading.Tasks.Task<TaskResponseDto> ImportFromJiraAsync(JiraImportDto dto);
}
