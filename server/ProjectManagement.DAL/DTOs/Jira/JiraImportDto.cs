namespace ProjectManagement.DAL;

public class JiraImportDto
{
    public string JiraIssueKey { get; set; } = string.Empty;
    public int ReleaseId { get; set; }
    public int AssignedToUserId { get; set; }
    public int AssignedToQAUserId { get; set; }
}
