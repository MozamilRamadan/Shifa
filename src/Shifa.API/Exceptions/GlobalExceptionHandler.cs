
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.Exceptions;

namespace Shifa.API.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
            if (exception is OperationCanceledException)
            {
                return false;
            }
            var (statusCode, title) = exception switch
            {
                ConflictException =>
                 (StatusCodes.Status409Conflict, "Conflict."),

                NotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found."),

            ForbiddenException =>
                (StatusCodes.Status403Forbidden, "Access denied."),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid request."),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        if (statusCode != StatusCodes.Status500InternalServerError)
        {
            problemDetails.Detail = exception.Message;
        }

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
