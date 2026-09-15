using MediatR;
using Microsoft.Extensions.Logging;

namespace CommunityHelper.Application.Common.Behaviours;

/// <summary>
/// Logs unexpected exceptions escaping a handler, then rethrows so the API's
/// exception handling middleware can translate them into Problem Details.
/// </summary>
public class UnhandledExceptionBehaviour<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for request {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
