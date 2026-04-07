using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Jira;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/jira")]
[Authorize]
public class JiraController : ControllerBase
{
    private readonly IJiraSettingsService _settingsService;
    private readonly IJiraService _jiraService;
    private readonly ITaskService _taskService;

    public JiraController(
        IJiraSettingsService settingsService,
        IJiraService jiraService,
        ITaskService taskService)
    {
        _settingsService = settingsService;
        _jiraService     = jiraService;
        _taskService     = taskService;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsService.GetAsync();
        return Ok(settings ?? new JiraSettingsDto());
    }

    [HttpPut("settings")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> UpsertSettings([FromBody] UpsertJiraSettingsDto dto)
    {
        var result = await _settingsService.UpsertAsync(dto);
        return Ok(result);
    }

    [HttpPost("settings/test")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> TestConnection()
    {
        var displayName = await _jiraService.TestConnectionAsync();
        return Ok(new { connectedAs = displayName });
    }

    [HttpGet("issues/search")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> SearchIssues(
        [FromQuery] string jql = "created >= -30d ORDER BY created DESC",
        [FromQuery] string? nextPageToken = null,
        [FromQuery] int maxResults = 50)
    {
        var result = await _jiraService.SearchIssuesAsync(jql, nextPageToken, maxResults);
        return Ok(result);
    }

    [HttpGet("issues/{issueKey}")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> GetIssue(string issueKey)
    {
        var issue = await _jiraService.GetIssueAsync(issueKey);
        return Ok(issue);
    }

    [HttpPost("import")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> ImportFromJira([FromBody] JiraImportDto dto)
    {
        var task = await _taskService.ImportFromJiraAsync(dto);
        return Ok(task);
    }
}
