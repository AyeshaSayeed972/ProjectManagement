using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.User;
using ProjectManagement.Enums;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, UserRole? role)
    {
        var (items, totalCount) = await _userRepository.GetFilteredPagedAsync(pageNumber, pageSize, role);

        return new PagedResult<UserResponseDto>
        {
            Data = items.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            }),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
