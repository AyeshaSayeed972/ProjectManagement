using TaskStatus = ProjectManagement.DAL.TaskStatus;

namespace ProjectManagement.DAL;

public class UpdateTaskStatusDto
{
    public TaskStatus NewStatus { get; set; }
}
