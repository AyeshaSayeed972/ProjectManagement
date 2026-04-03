using ProjectManagement.Enums;

namespace ProjectManagement.DTOs.User;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
