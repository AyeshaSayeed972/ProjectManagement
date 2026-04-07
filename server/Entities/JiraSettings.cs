namespace ProjectManagement.Entities;

public class JiraSettings
{
    public int Id { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
