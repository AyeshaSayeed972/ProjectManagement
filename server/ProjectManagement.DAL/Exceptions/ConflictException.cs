namespace ProjectManagement.DAL;

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}
