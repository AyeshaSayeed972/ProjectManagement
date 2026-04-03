using ProjectManagement.Enums;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PRLink { get; set; }
    public string? Remarks { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ReleaseId { get; set; }
    public Release Release { get; set; } = null!;

    public int AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = null!;

    public int? AssignedToQAUserId { get; set; }
    public User? AssignedToQAUser { get; set; }
}
