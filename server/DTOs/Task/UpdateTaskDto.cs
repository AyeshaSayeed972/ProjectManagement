namespace ProjectManagement.DTOs.Task;

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public int AssignedToUserId { get; set; }
    public int AssignedToQAUserId { get; set; }
}
