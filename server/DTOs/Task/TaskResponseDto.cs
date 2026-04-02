using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.DTOs.Task;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseId { get; set; }
    public int AssignedToUserId { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public string? PRLink { get; set; }
    public string? ApproverName { get; set; }
    public string? Remarks { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
