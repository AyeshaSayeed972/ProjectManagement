using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.BLL;

public interface IReleaseService
{
    System.Threading.Tasks.Task<PagedResult<ReleaseResponseDto>> GetAllAsync(int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ReleaseResponseDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<ReleaseResponseDto> CreateAsync(CreateReleaseDto dto);
    System.Threading.Tasks.Task<ReleaseResponseDto> UpdateAsync(int id, UpdateReleaseDto dto);
    Task DeleteAsync(int id);
}
