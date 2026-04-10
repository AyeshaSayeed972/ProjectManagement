using Microsoft.AspNetCore.Identity;

namespace ProjectManagement.Entities;

public class User : IdentityUser<int>
{
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public ICollection<Task> QAAssignedTasks { get; set; } = new List<Task>();
}
