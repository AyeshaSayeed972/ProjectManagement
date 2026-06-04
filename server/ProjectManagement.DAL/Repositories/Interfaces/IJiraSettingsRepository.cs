namespace ProjectManagement.DAL;

public interface IJiraSettingsRepository
{
    System.Threading.Tasks.Task<JiraSettings?> GetAsync();
    System.Threading.Tasks.Task UpsertAsync(JiraSettings settings);
}
