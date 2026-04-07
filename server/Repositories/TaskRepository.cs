using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Task;
using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

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
            .Include(t => t.AssignedToQAUser)
            .Include(t => t.Release)
            .FirstOrDefaultAsync(t => t.Id == id);

    // Counts on the filtered table (no JOIN needed), then loads one page via projection.
    // EF generates a LEFT JOIN to Users for AssignedToUser.Username.
    public async Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetByReleaseIdPagedAsync(
        int releaseId, int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null)
    {
        var query = _context.Tasks.Where(t => t.ReleaseId == releaseId);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (!string.IsNullOrEmpty(assignedToUsername))
{
    if (userRole == "QA")
    {
        query = query.Where(t =>
            t.AssignedToQAUser != null &&
            t.AssignedToQAUser.Username == assignedToUsername
        );
    }
    else
    {
        query = query.Where(t =>
            t.AssignedToUser.Username == assignedToUsername
        );
    }
}

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponseDto
            {
                Id                   = t.Id,
                Title                = t.Title,
                ReleaseId            = t.ReleaseId,
                ReleaseTitle         = t.Release.Title,
                AssignedToUserId     = t.AssignedToUserId,
                AssignedToUsername   = t.AssignedToUser.Username,
                AssignedToQAUserId   = t.AssignedToQAUserId,
                AssignedToQAUsername = t.AssignedToQAUser == null ? (string?)null : t.AssignedToQAUser.Username,
                PRLink               = t.PRLink,
                Remarks              = t.Remarks,
                JiraIssueKey         = t.JiraIssueKey,
                Status               = t.Status,
                CreatedAt            = t.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<TaskResponseDto> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, TaskStatus? status = null, string? assignedToUsername = null, string? userRole = null)
    {
        var query = _context.Tasks.AsQueryable();
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
                if (!string.IsNullOrEmpty(assignedToUsername))
{
    if (userRole == "QA")
    {
        query = query.Where(t =>
            t.AssignedToQAUser != null &&
            t.AssignedToQAUser.Username == assignedToUsername
        );
    }
    else
    {
        query = query.Where(t =>
            t.AssignedToUser.Username == assignedToUsername
        );
    }
}

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponseDto
            {
                Id                   = t.Id,
                Title                = t.Title,
                ReleaseId            = t.ReleaseId,
                ReleaseTitle         = t.Release.Title,
                AssignedToUserId     = t.AssignedToUserId,
                AssignedToUsername   = t.AssignedToUser.Username,
                AssignedToQAUserId   = t.AssignedToQAUserId,
                AssignedToQAUsername = t.AssignedToQAUser == null ? (string?)null : t.AssignedToQAUser.Username,
                PRLink               = t.PRLink,
                Remarks              = t.Remarks,
                JiraIssueKey         = t.JiraIssueKey,
                Status               = t.Status,
                CreatedAt            = t.CreatedAt
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
