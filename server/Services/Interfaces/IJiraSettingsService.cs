using ProjectManagement.DTOs.Jira;

namespace ProjectManagement.Services.Interfaces;

public interface IJiraSettingsService
{
    Task<JiraSettingsDto?> GetAsync();
    Task<JiraSettingsDto> UpsertAsync(UpsertJiraSettingsDto dto);
}
