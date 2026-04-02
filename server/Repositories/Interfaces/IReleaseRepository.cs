using ProjectManagement.DTOs.Release;
using ProjectManagement.Entities;

namespace ProjectManagement.Repositories.Interfaces;

public interface IReleaseRepository
{
    System.Threading.Tasks.Task<Release?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<ReleaseResponseDto?> GetByIdProjectedAsync(int id);
    System.Threading.Tasks.Task<(IEnumerable<ReleaseResponseDto> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    System.Threading.Tasks.Task<Release> AddAsync(Release release);
    System.Threading.Tasks.Task UpdateAsync(Release release);
    System.Threading.Tasks.Task DeleteAsync(Release release);
}
