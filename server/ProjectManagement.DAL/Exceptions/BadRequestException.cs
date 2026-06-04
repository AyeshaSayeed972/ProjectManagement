namespace ProjectManagement.DAL;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, 400) { }
}
