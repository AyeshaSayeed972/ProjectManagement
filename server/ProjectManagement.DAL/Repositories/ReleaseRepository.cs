using Microsoft.EntityFrameworkCore;

namespace ProjectManagement.DAL;

public class ReleaseRepository : IReleaseRepository
{
    private readonly AppDbContext _context;

    public ReleaseRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns bare entity — used by write paths (Update, Delete) that do not need Tasks loaded.
    public async System.Threading.Tasks.Task<Release?> GetByIdAsync(int id)
        => await _context.Releases.FirstOrDefaultAsync(r => r.Id == id);

    // Projects directly to DTO — used by the read endpoint (GET /releases/{id}).
    public async System.Threading.Tasks.Task<ReleaseResponseDto?> GetByIdProjectedAsync(int id)
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

    public async System.Threading.Tasks.Task<(IEnumerable<ReleaseResponseDto> Items, int TotalCount)> GetPagedAsync(
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

    public async System.Threading.Tasks.Task<Release> AddAsync(Release release)
    {
        _context.Releases.Add(release);
        await _context.SaveChangesAsync();
        return release;
    }

    public async System.Threading.Tasks.Task UpdateAsync(Release release)
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

    public async System.Threading.Tasks.Task DeleteAsync(Release release)
    {
        _context.Releases.Remove(release);
        await _context.SaveChangesAsync();
    }
}
