using ProjectManagement.DTOs.Jira;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Services.Interfaces;

public interface IJiraService
{
    Task<JiraIssueDto> GetIssueAsync(string issueKey);
    Task<JiraSearchResultDto> SearchIssuesAsync(string jql, string? nextPageToken = null, int maxResults = 50);
    Task<string> CreateIssueAsync(string projectKey, string summary, string issueType, string? description);
    Task<string> TestConnectionAsync();
}
