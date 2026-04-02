using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Release;
using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services;

public class ReleaseService : IReleaseService
{
    private readonly IReleaseRepository _releaseRepository;

    public ReleaseService(IReleaseRepository releaseRepository)
    {
        _releaseRepository = releaseRepository;
    }

    public async Task<PagedResult<ReleaseResponseDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _releaseRepository.GetPagedAsync(pageNumber, pageSize);
        return new PagedResult<ReleaseResponseDto>
        {
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ReleaseResponseDto?> GetByIdAsync(int id)
        => await _releaseRepository.GetByIdProjectedAsync(id);

    public async Task<ReleaseResponseDto> CreateAsync(CreateReleaseDto dto)
    {
        var release = new Entities.Release
        {
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = ReleaseStatus.Upcoming,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _releaseRepository.AddAsync(release);
        return (await _releaseRepository.GetByIdProjectedAsync(created.Id))!;
    }

    public async Task<ReleaseResponseDto> UpdateAsync(int id, UpdateReleaseDto dto)
    {
        var release = await _releaseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Release with id {id} not found.");

        release.Title = dto.Title;
        release.Description = dto.Description;
        release.StartDate = dto.StartDate;
        release.EndDate = dto.EndDate;
        release.Status = dto.Status;

        await _releaseRepository.UpdateAsync(release);
        return (await _releaseRepository.GetByIdProjectedAsync(id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var release = await _releaseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Release with id {id} not found.");

        await _releaseRepository.DeleteAsync(release);
    }
}
