using System.Text.Json;
using ProjectManagement.Exceptions;

namespace ProjectManagement.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Expected application errors are business conditions, not bugs — log at Warning.
            // Everything else is unexpected and should be investigated — log at Error.
            if (ex is AppException appException)
                _logger.LogWarning("Application exception {StatusCode}: {Message}",
                    appException.StatusCode, appException.Message);
            else
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception is AppException appEx
            ? (appEx.StatusCode, appEx.Message)
            : (500, "An unexpected error occurred.");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = JsonSerializer.Serialize(
            new { success = false, statusCode, message },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(body);
    }
}
