namespace ProjectManagement.DTOs.Jira;

public class JiraSettingsDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}
