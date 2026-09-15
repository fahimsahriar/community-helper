using System.Text.Json;
using CommunityHelper.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FluentValidation.ValidationException;

namespace CommunityHelper.API.Middleware;

/// <summary>
/// Translates unhandled exceptions into RFC 7807 Problem Details responses.
/// Stack traces are never returned to clients.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Response already started; cannot write Problem Details.");
            throw exception;
        }

        var problem = BuildProblemDetails(exception);
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Request to {Path} failed with {StatusCode}: {Message}",
                context.Request.Path,
                problem.Status,
                exception.Message);
        }

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        // Serialize against the runtime type: ValidationProblemDetails.Errors is
        // declared on the derived type and is dropped if we serialize as ProblemDetails.
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, problem.GetType(), JsonOptions),
            context.RequestAborted);
    }

    private ProblemDetails BuildProblemDetails(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(
            validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        },

        NotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found.",
            Detail = notFound.Message,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        },

        UnauthorizedAccessException => new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        },

        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            // Exception messages can carry internals — only surface them outside production.
            Detail = environment.IsProduction() ? null : exception.Message,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        },
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
