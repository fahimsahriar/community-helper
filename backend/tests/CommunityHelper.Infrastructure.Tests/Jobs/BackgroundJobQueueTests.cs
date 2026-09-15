using CommunityHelper.Application.Common.Models;
using CommunityHelper.Infrastructure.Jobs;
using MediatR;

namespace CommunityHelper.Infrastructure.Tests.Jobs;

public class BackgroundJobQueueTests
{
    private record NoopRequest : IRequest<string>;

    private static JobWorkItem Item(string jobId) =>
        new(jobId, "connection-1", new NoopRequest());

    [Fact]
    public async Task DequeueAllAsync_YieldsItemsInTheOrderTheyWereEnqueued()
    {
        var queue = new BackgroundJobQueue();
        await queue.EnqueueAsync(Item("first"));
        await queue.EnqueueAsync(Item("second"));

        var dequeued = new List<string>();

        await foreach (var item in queue.DequeueAllAsync(CancellationToken.None))
        {
            dequeued.Add(item.JobId);
            if (dequeued.Count == 2)
            {
                break;
            }
        }

        Assert.Equal(["first", "second"], dequeued);
    }

    [Fact]
    public async Task DequeueAllAsync_WaitsForWorkRatherThanCompleting()
    {
        var queue = new BackgroundJobQueue();
        using var cts = new CancellationTokenSource();

        var consumer = Task.Run(async () =>
        {
            await foreach (var item in queue.DequeueAllAsync(cts.Token))
            {
                return item.JobId;
            }

            return null;
        });

        Assert.False(consumer.IsCompleted);

        await queue.EnqueueAsync(Item("late-arrival"));

        Assert.Equal("late-arrival", await consumer);
    }
}
