using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Common.Models;
using CommunityHelper.Application.Features.Opportunities.Commands.SubmitOpportunitySearch;
using CommunityHelper.Application.Features.Opportunities.Queries.SearchOpportunities;

namespace CommunityHelper.Application.Tests.Features.Opportunities;

public class SubmitOpportunitySearchCommandTests
{
    private sealed class CapturingQueue : IBackgroundJobQueue
    {
        public List<JobWorkItem> Enqueued { get; } = [];

        public ValueTask EnqueueAsync(JobWorkItem item, CancellationToken ct = default)
        {
            Enqueued.Add(item);
            return ValueTask.CompletedTask;
        }

        public IAsyncEnumerable<JobWorkItem> DequeueAllAsync(CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class StubRegistry(params string[] connected) : IClientConnectionRegistry
    {
        public bool IsConnected(string connectionId) => connected.Contains(connectionId);

        public Task<bool> SendAsync(string connectionId, SocketMessage message, CancellationToken ct = default) =>
            Task.FromResult(true);
    }

    [Fact]
    public async Task Handle_QueuesTheSearchAgainstTheCallersConnection()
    {
        var queue = new CapturingQueue();
        var handler = new SubmitOpportunitySearchCommandHandler(queue);

        var jobId = await handler.Handle(
            new SubmitOpportunitySearchCommand("conn-1", "tutoring"),
            CancellationToken.None);

        var queued = Assert.Single(queue.Enqueued);
        Assert.Equal(jobId, queued.JobId);
        Assert.Equal("conn-1", queued.ConnectionId);
        var request = Assert.IsType<SearchOpportunitiesQuery>(queued.Request);
        Assert.Equal("tutoring", request.Query);
    }

    [Fact]
    public async Task Handle_ReturnsANonEmptyJobIdThatDiffersPerSubmission()
    {
        var handler = new SubmitOpportunitySearchCommandHandler(new CapturingQueue());
        var command = new SubmitOpportunitySearchCommand("conn-1", "tutoring");

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.NotEmpty(first);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Validator_RejectsAConnectionIdThatIsNotOpen()
    {
        var validator = new SubmitOpportunitySearchCommandValidator(new StubRegistry("conn-open"));

        var result = validator.Validate(new SubmitOpportunitySearchCommand("conn-closed", "tutoring"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitOpportunitySearchCommand.ConnectionId));
    }

    [Fact]
    public void Validator_RejectsAMissingConnectionId()
    {
        var validator = new SubmitOpportunitySearchCommandValidator(new StubRegistry("conn-open"));

        var result = validator.Validate(new SubmitOpportunitySearchCommand("", "tutoring"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_RejectsAnOverlongQuery()
    {
        var validator = new SubmitOpportunitySearchCommandValidator(new StubRegistry("conn-open"));

        var result = validator.Validate(
            new SubmitOpportunitySearchCommand("conn-open", new string('x', 201)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitOpportunitySearchCommand.Query));
    }

    [Fact]
    public void Validator_AcceptsAnOpenConnectionAndAnEmptyQuery()
    {
        var validator = new SubmitOpportunitySearchCommandValidator(new StubRegistry("conn-open"));

        var result = validator.Validate(new SubmitOpportunitySearchCommand("conn-open", ""));

        Assert.True(result.IsValid);
    }
}
