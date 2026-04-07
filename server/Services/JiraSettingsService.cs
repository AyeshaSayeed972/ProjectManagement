using ProjectManagement.DTOs.Jira;
using ProjectManagement.Entities;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services;

public class JiraSettingsService : IJiraSettingsService
{
    private readonly IJiraSettingsRepository _repository;

    public JiraSettingsService(IJiraSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<JiraSettingsDto?> GetAsync()
    {
        var settings = await _repository.GetAsync();
        if (settings is null) return null;
        return MapToDto(settings);
    }

    public async Task<JiraSettingsDto> UpsertAsync(UpsertJiraSettingsDto dto)
    {
        var settings = new JiraSettings
        {
            BaseUrl   = dto.BaseUrl.TrimEnd('/'),
            Email     = dto.Email,
            ApiToken  = dto.ApiToken,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.UpsertAsync(settings);
        return MapToDto(settings);
    }

    private static JiraSettingsDto MapToDto(JiraSettings settings) => new()
    {
        BaseUrl      = settings.BaseUrl,
        Email        = settings.Email,
        IsConfigured = !string.IsNullOrWhiteSpace(settings.BaseUrl)
                    && !string.IsNullOrWhiteSpace(settings.Email)
                    && !string.IsNullOrWhiteSpace(settings.ApiToken)
    };
}
