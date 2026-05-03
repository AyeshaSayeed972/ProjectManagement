using ProjectManagement.DAL;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagement.BLL;

public interface IAuthService
{
    System.Threading.Tasks.Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
}
