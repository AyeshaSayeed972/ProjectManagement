namespace ProjectManagement.DTOs.Task;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public int ReleaseId { get; set; }
    public int AssignedToUserId { get; set; }
}
