using CommunityHelper.Application.Common.Models;

namespace CommunityHelper.Application.Common.Interfaces;

public interface IBackgroundJobQueue
{
    ValueTask EnqueueAsync(JobWorkItem item, CancellationToken ct = default);

    /// <summary>Yields queued work until <paramref name="ct"/> is cancelled.</summary>
    IAsyncEnumerable<JobWorkItem> DequeueAllAsync(CancellationToken ct);
}
