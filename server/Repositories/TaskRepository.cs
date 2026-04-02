using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;

namespace ProjectManagement.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns full entity with AssignedToUser loaded.
    // All write paths mutate the entity then call MapToDto, which accesses AssignedToUser.Username.
    public async Task<Entities.Task?> GetByIdAsync(int id)
        => await _context.Tasks
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

    // Counts on the filtered table (no JOIN needed), then loads one page via projection.
    // EF generates a LEFT JOIN to Users for AssignedToUser.Username.
    public async Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetByReleaseIdPagedAsync(
        int releaseId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Tasks
            .CountAsync(t => t.ReleaseId == releaseId);

        var items = await _context.Tasks
            .Where(t => t.ReleaseId == releaseId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponseDto
            {
                Id                 = t.Id,
                Title              = t.Title,
                ReleaseId          = t.ReleaseId,
                AssignedToUserId   = t.AssignedToUserId,
                AssignedToUsername = t.AssignedToUser.Username,
                PRLink             = t.PRLink,
                ApproverName       = t.ApproverName,
                Remarks            = t.Remarks,
                Status             = t.Status,
                CreatedAt          = t.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Entities.Task> AddAsync(Entities.Task task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(Entities.Task task)
    {
        try
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The task was modified by another request. Please retry.");
        }
    }

    public async Task DeleteAsync(Entities.Task task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
