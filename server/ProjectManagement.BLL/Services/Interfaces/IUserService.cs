using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.BLL;

public interface IUserService
{
    System.Threading.Tasks.Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, UserRole? role);
}
