using System.Runtime.CompilerServices;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Common.Models;
using CommunityHelper.Infrastructure.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CommunityHelper.Infrastructure.Tests.Jobs;

public class JobProcessorServiceTests
{
    private record DemoRequest(string Value) : IRequest<string>;

    /// <summary>Yields a fixed set of work items, then ends the loop.</summary>
    private sealed class FakeQueue(params JobWorkItem[] items) : IBackgroundJobQueue
    {
        public ValueTask EnqueueAsync(JobWorkItem item, CancellationToken ct = default) =>
            ValueTask.CompletedTask;

        public async IAsyncEnumerable<JobWorkItem> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();
                yield return item;
            }

            await Task.CompletedTask;
        }
    }

    private sealed class RecordingRegistry : IClientConnectionRegistry
    {
        public List<(string ConnectionId, SocketMessage Message)> Sent { get; } = [];

        public bool IsConnected(string connectionId) => true;

        public Task<bool> SendAsync(string connectionId, SocketMessage message, CancellationToken ct = default)
        {
            Sent.Add((connectionId, message));
            return Task.FromResult(true);
        }
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static async Task<RecordingRegistry> RunAsync(
        Func<object, Task<object?>> handle,
        string? environmentName = null,
        params JobWorkItem[] items)
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns((object request, CancellationToken _) => handle(request));

        var provider = new ServiceCollection()
            .AddSingleton(sender.Object)
            .BuildServiceProvider();

        var registry = new RecordingRegistry();

        var service = new JobProcessorService(
            new FakeQueue(items),
            provider.GetRequiredService<IServiceScopeFactory>(),
            registry,
            new TestHostEnvironment(environmentName ?? Environments.Development),
            NullLogger<JobProcessorService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await service.ExecuteTask!;
        await service.StopAsync(CancellationToken.None);

        return registry;
    }

    [Fact]
    public async Task Job_PushesRunningThenCompletedWithTheHandlerResult()
    {
        var item = new JobWorkItem("job-1", "conn-1", new DemoRequest("hello"));

        var registry = await RunAsync(_ => Task.FromResult<object?>("the result"), items: item);

        Assert.Equal(
            [SocketMessageTypes.JobRunning, SocketMessageTypes.JobCompleted],
            registry.Sent.Select(s => s.Message.Type));
        Assert.All(registry.Sent, s => Assert.Equal("conn-1", s.ConnectionId));
        Assert.All(registry.Sent, s => Assert.Equal("job-1", s.Message.JobId));
        Assert.Equal("the result", registry.Sent[1].Message.Result);
    }

    [Fact]
    public async Task Job_PushesFailedWhenTheHandlerThrows()
    {
        var item = new JobWorkItem("job-1", "conn-1", new DemoRequest("boom"));

        var registry = await RunAsync(
            _ => throw new InvalidOperationException("handler exploded"),
            items: item);

        Assert.Equal(
            [SocketMessageTypes.JobRunning, SocketMessageTypes.JobFailed],
            registry.Sent.Select(s => s.Message.Type));
        Assert.Equal("handler exploded", registry.Sent[1].Message.Error);
        Assert.Null(registry.Sent[1].Message.Result);
    }

    [Fact]
    public async Task Job_DoesNotLeakExceptionDetailInProduction()
    {
        var item = new JobWorkItem("job-1", "conn-1", new DemoRequest("boom"));

        var registry = await RunAsync(
            _ => throw new InvalidOperationException("connection string is bad"),
            Environments.Production,
            item);

        var failure = registry.Sent[1].Message;
        Assert.Equal(SocketMessageTypes.JobFailed, failure.Type);
        Assert.DoesNotContain("connection string", failure.Error);
    }

    [Fact]
    public async Task Worker_KeepsProcessingAfterAJobFails()
    {
        var failing = new JobWorkItem("job-bad", "conn-1", new DemoRequest("boom"));
        var succeeding = new JobWorkItem("job-good", "conn-1", new DemoRequest("fine"));

        var registry = await RunAsync(
            request => ((DemoRequest)request).Value == "boom"
                ? throw new InvalidOperationException("nope")
                : Task.FromResult<object?>("ok"),
            items: [failing, succeeding]);

        var lastForGoodJob = registry.Sent.Last(s => s.Message.JobId == "job-good").Message;
        Assert.Equal(SocketMessageTypes.JobCompleted, lastForGoodJob.Type);
        Assert.Equal("ok", lastForGoodJob.Result);
    }

    [Fact]
    public async Task Job_RoutesEveryMessageToTheSubmittingConnectionOnly()
    {
        var first = new JobWorkItem("job-1", "conn-alice", new DemoRequest("a"));
        var second = new JobWorkItem("job-2", "conn-bob", new DemoRequest("b"));

        var registry = await RunAsync(
            _ => Task.FromResult<object?>("done"),
            items: [first, second]);

        Assert.All(
            registry.Sent.Where(s => s.Message.JobId == "job-1"),
            s => Assert.Equal("conn-alice", s.ConnectionId));
        Assert.All(
            registry.Sent.Where(s => s.Message.JobId == "job-2"),
            s => Assert.Equal("conn-bob", s.ConnectionId));
    }
}
