using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityHelper.Infrastructure.Jobs;

/// <summary>
/// Drains the job queue, dispatches each work item through MediatR in its own
/// DI scope, and pushes the outcome to the submitting socket connection.
/// </summary>
public sealed class JobProcessorService(
    IBackgroundJobQueue queue,
    IServiceScopeFactory scopeFactory,
    IClientConnectionRegistry connections,
    IHostEnvironment environment,
    ILogger<JobProcessorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Job processor started");

        await foreach (var item in queue.DequeueAllAsync(stoppingToken))
        {
            // ProcessAsync never throws, so one bad job cannot stop the worker.
            await ProcessAsync(item, stoppingToken);
        }
    }

    private async Task ProcessAsync(JobWorkItem item, CancellationToken ct)
    {
        await connections.SendAsync(
            item.ConnectionId,
            new SocketMessage { Type = SocketMessageTypes.JobRunning, JobId = item.JobId },
            ct);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await sender.Send(item.Request, ct);

            logger.LogInformation(
                "Job {JobId} ({RequestName}) completed",
                item.JobId,
                item.Request.GetType().Name);

            await connections.SendAsync(
                item.ConnectionId,
                new SocketMessage
                {
                    Type = SocketMessageTypes.JobCompleted,
                    JobId = item.JobId,
                    Result = result,
                },
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Host is shutting down; the client will time out and retry.
            logger.LogInformation("Job {JobId} abandoned during shutdown", item.JobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobId} failed", item.JobId);

            await connections.SendAsync(
                item.ConnectionId,
                new SocketMessage
                {
                    Type = SocketMessageTypes.JobFailed,
                    JobId = item.JobId,
                    // Same rule as ExceptionHandlingMiddleware: no internals in production.
                    Error = environment.IsProduction()
                        ? "The job could not be completed."
                        : ex.Message,
                },
                CancellationToken.None);
        }
    }
}
