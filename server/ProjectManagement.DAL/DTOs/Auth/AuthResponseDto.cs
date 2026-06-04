namespace ProjectManagement.DAL;

public class AuthResponseDto
{
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
