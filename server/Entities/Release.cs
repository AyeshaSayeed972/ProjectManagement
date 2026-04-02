using ProjectManagement.Enums;

namespace ProjectManagement.Entities;

public class Release
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReleaseStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
