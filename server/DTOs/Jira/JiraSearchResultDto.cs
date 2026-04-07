namespace ProjectManagement.DTOs.Jira;

public class JiraSearchResultDto
{
    public List<JiraIssueDto> Issues { get; set; } = new();
    public string? NextPageToken { get; set; }
    public bool IsLast { get; set; }
}
