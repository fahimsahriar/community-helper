using CommunityHelper.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityHelper.Infrastructure.Services;

/// <summary>
/// Creates MongoDB indexes at startup. Registered only when MongoDB is configured.
/// </summary>
public sealed class MongoIndexSetup(
    MongoDbContext context,
    ILogger<MongoIndexSetup> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Ensuring MongoDB indexes.");
        await context.EnsureIndexesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
