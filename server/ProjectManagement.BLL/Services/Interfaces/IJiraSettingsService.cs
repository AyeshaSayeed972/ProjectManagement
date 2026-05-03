using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.BLL;

public interface IJiraSettingsService
{
    System.Threading.Tasks.Task<JiraSettingsDto?> GetAsync();
    System.Threading.Tasks.Task<JiraSettingsDto> UpsertAsync(UpsertJiraSettingsDto dto);
}
