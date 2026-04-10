using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Release;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;

namespace ProjectManagement.Repositories;

public class ReleaseRepository : IReleaseRepository
{
    private readonly AppDbContext _context;

    public ReleaseRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns bare entity — used by write paths (Update, Delete) that do not need Tasks loaded.
    public async Task<Entities.Release?> GetByIdAsync(int id)
        => await _context.Releases.FirstOrDefaultAsync(r => r.Id == id);

    // Projects directly to DTO — used by the read endpoint (GET /releases/{id}).
    // EF generates a correlated subquery for Tasks and JOINs Users for Username,
    // selecting only the columns present in the projection.
    public async Task<ReleaseResponseDto?> GetByIdProjectedAsync(int id)
        => await _context.Releases
            .Where(r => r.Id == id)
            .Select(r => new ReleaseResponseDto
            {
                Id          = r.Id,
                Title       = r.Title,
                Description = r.Description,
                StartDate   = r.StartDate,
                EndDate     = r.EndDate,
                Status      = r.Status,
                CreatedAt   = r.CreatedAt,
                Tasks = r.Tasks.Select(t => new TaskSummaryDto
                {
                    Id                 = t.Id,
                    Title              = t.Title,
                    AssignedToUsername = t.AssignedToUser.UserName!,
                    Status             = t.Status
                }).ToList()
            })
            .FirstOrDefaultAsync();

    // Counts on the bare table (no JOINs), then loads one page via projection.
    public async Task<(IEnumerable<ReleaseResponseDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize)
    {
        var totalCount = await _context.Releases.CountAsync();

        var items = await _context.Releases
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReleaseResponseDto
            {
                Id          = r.Id,
                Title       = r.Title,
                Description = r.Description,
                StartDate   = r.StartDate,
                EndDate     = r.EndDate,
                Status      = r.Status,
                CreatedAt   = r.CreatedAt,
                Tasks = r.Tasks.Select(t => new TaskSummaryDto
                {
                    Id                 = t.Id,
                    Title              = t.Title,
                    AssignedToUsername = t.AssignedToUser.UserName!,
                    Status             = t.Status
                }).ToList()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Entities.Release> AddAsync(Entities.Release release)
    {
        _context.Releases.Add(release);
        await _context.SaveChangesAsync();
        return release;
    }

    public async Task UpdateAsync(Entities.Release release)
    {
        try
        {
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The release was modified by another request. Please retry.");
        }
    }

    public async Task DeleteAsync(Entities.Release release)
    {
        _context.Releases.Remove(release);
        await _context.SaveChangesAsync();
    }
}
