using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Release;

namespace ProjectManagement.Services.Interfaces;

public interface IReleaseService
{
    Task<PagedResult<ReleaseResponseDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<ReleaseResponseDto?> GetByIdAsync(int id);
    Task<ReleaseResponseDto> CreateAsync(CreateReleaseDto dto);
    Task<ReleaseResponseDto> UpdateAsync(int id, UpdateReleaseDto dto);
    Task DeleteAsync(int id);
}
