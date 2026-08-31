using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NovaKart.WebApi.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

        _logger.LogError(
            exception,
            "Unhandled exception occurred while processing request {RequestMethod} {RequestPath}. " +
            "TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            Activity.Current?.Id ?? httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = httpContext.Request.Path,
            Extensions = new Dictionary<string, object?>
            {
                ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
            }
        };

        if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
        {
            problemDetails.Extensions["requestId"] = httpContext.TraceIdentifier;
        }

        if (_environment.IsDevelopment())
        {
            problemDetails.Title = exception.GetType().Name;
            problemDetails.Detail = exception.Message;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}