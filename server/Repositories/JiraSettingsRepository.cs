using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Repositories.Interfaces;

namespace ProjectManagement.Repositories;

public class JiraSettingsRepository : IJiraSettingsRepository
{
    private readonly AppDbContext _context;

    public JiraSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<JiraSettings?> GetAsync()
        => await _context.JiraSettings.FirstOrDefaultAsync();

    public async System.Threading.Tasks.Task UpsertAsync(JiraSettings settings)
    {
        var existing = await _context.JiraSettings.FirstOrDefaultAsync();
        if (existing is null)
        {
            settings.Id = 1;
            _context.JiraSettings.Add(settings);
        }
        else
        {
            existing.BaseUrl   = settings.BaseUrl;
            existing.Email     = settings.Email;
            existing.ApiToken  = settings.ApiToken;
            existing.UpdatedAt = settings.UpdatedAt;
            _context.JiraSettings.Update(existing);
        }
        await _context.SaveChangesAsync();
    }
}
