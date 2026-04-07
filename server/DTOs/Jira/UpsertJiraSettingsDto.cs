namespace ProjectManagement.DTOs.Jira;

public class UpsertJiraSettingsDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
}
