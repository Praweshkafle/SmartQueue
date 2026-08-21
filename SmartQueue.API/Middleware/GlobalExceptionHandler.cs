using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;

namespace SmartQueue.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        Console.WriteLine($"GlobalExceptionHandler fired: {exception.GetType().Name}");
        var correlationId = context.Response.Headers["X-Correlation-ID"]
            .FirstOrDefault() ?? "unknown";

        var (statusCode, message) = exception switch
        {
            ValidationException => (400, "Validation failed."),
            UnauthorizedAccessException => (401, "Unauthorized."),
            _ => (500, "An unexpected error occurred.")
        };

        if (statusCode == 500)
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
        else
            _logger.LogWarning(
                "Handled exception {ExceptionType}. CorrelationId: {CorrelationId}",
                exception.GetType().Name, correlationId);

        context.Response.StatusCode = statusCode;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .Select(e => new { field = e.PropertyName, error = e.ErrorMessage });

            await context.Response.WriteAsJsonAsync(new
            {
                status = statusCode,
                message,
                correlationId,
                errors
            }, ct);
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new
            {
                status = statusCode,
                message,
                correlationId
            }, ct);
        }

        return true;
    }
}