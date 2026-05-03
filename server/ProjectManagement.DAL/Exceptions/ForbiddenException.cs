namespace ProjectManagement.DAL;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
