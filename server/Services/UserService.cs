using Microsoft.AspNetCore.Identity;
using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.User;
using ProjectManagement.Entities;
using ProjectManagement.Enums;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;

    public UserService(IUserRepository userRepository, UserManager<User> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, UserRole? role)
    {
        var (items, totalCount) = await _userRepository.GetFilteredPagedAsync(pageNumber, pageSize, role);

        var dtos = new List<UserResponseDto>();
        foreach (var u in items)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserResponseDto
            {
                Id       = u.Id,
                Username = u.UserName!,
                Role     = Enum.Parse<UserRole>(roles.FirstOrDefault() ?? nameof(UserRole.Developer))
            });
        }

        return new PagedResult<UserResponseDto>
        {
            Data       = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize   = pageSize
        };
    }
}
