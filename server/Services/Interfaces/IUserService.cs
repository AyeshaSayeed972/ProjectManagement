using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.User;
using ProjectManagement.Enums;

namespace ProjectManagement.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, UserRole? role);
}
