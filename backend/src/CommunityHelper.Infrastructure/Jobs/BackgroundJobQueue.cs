using System.Threading.Channels;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Common.Models;

namespace CommunityHelper.Infrastructure.Jobs;

/// <summary>
/// In-process job queue. Bounded so a burst of submissions applies backpressure
/// to the HTTP layer instead of growing until the process runs out of memory:
/// once full, <see cref="EnqueueAsync"/> waits for room.
/// Queued work does not survive a restart — see docs/adr/0001.
/// </summary>
public sealed class BackgroundJobQueue : IBackgroundJobQueue
{
    private const int Capacity = 100;

    private readonly Channel<JobWorkItem> _channel = Channel.CreateBounded<JobWorkItem>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });

    public ValueTask EnqueueAsync(JobWorkItem item, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(item, ct);

    public IAsyncEnumerable<JobWorkItem> DequeueAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
