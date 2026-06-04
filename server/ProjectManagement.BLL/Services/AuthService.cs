using Microsoft.AspNetCore.Identity;
using ProjectManagement.DAL;

namespace ProjectManagement.BLL;

public class AuthService : IAuthService
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthService(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
    }

    public async System.Threading.Tasks.Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.Username, dto.Password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
            throw new ForbiddenException("Invalid username or password.");

        var user  = await _userManager.FindByNameAsync(dto.Username);
        var roles = await _userManager.GetRolesAsync(user!);

        return new AuthResponseDto
        {
            Username = user!.UserName!,
            Role     = Enum.Parse<UserRole>(roles.First())
        };
    }
}
