using ProjectManagement.Enums;

namespace ProjectManagement.DTOs.Auth;

public class AuthResponseDto
{
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
