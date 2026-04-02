using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.DTOs.Task;

public class UpdateTaskStatusDto
{
    public TaskStatus NewStatus { get; set; }
}
