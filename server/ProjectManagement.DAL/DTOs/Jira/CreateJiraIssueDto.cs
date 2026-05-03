namespace ProjectManagement.DAL;

public class CreateJiraIssueDto
{
    public string ProjectKey { get; set; } = string.Empty;
    public string? IssueType { get; set; }
    public string? Description { get; set; }
}
