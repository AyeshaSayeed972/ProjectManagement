using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.BLL;

public interface IJiraService
{
    System.Threading.Tasks.Task<JiraIssueDto> GetIssueAsync(string issueKey);
    System.Threading.Tasks.Task<JiraSearchResultDto> SearchIssuesAsync(string jql, string? nextPageToken = null, int maxResults = 50);
    System.Threading.Tasks.Task<string> CreateIssueAsync(string projectKey, string summary, string issueType, string? description);
    System.Threading.Tasks.Task<string> TestConnectionAsync();
}
