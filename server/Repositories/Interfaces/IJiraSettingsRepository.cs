using ProjectManagement.Entities;

namespace ProjectManagement.Repositories.Interfaces;

public interface IJiraSettingsRepository
{
    System.Threading.Tasks.Task<JiraSettings?> GetAsync();
    System.Threading.Tasks.Task UpsertAsync(JiraSettings settings);
}
