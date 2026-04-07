namespace ProjectManagement.DTOs.Jira;

public class JiraIssueDto
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public string? IssueType { get; set; }
    public string BrowseUrl { get; set; } = string.Empty;
}
