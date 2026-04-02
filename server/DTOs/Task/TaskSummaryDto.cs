using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.DTOs.Task;

public class TaskSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AssignedToUsername { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}
