using TaskStatus = ProjectManagement.DAL.TaskStatus;

namespace ProjectManagement.DAL;

public class TaskSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AssignedToUsername { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}
