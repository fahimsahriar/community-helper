using CommunityHelper.Application.Common.Models;
using CommunityHelper.Infrastructure.Realtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityHelper.Infrastructure.Tests.Realtime;

public class WebSocketConnectionRegistryTests
{
    private static WebSocketConnectionRegistry CreateRegistry() =>
        new(NullLogger<WebSocketConnectionRegistry>.Instance);

    [Fact]
    public void IsConnected_IsFalseForAnUnknownConnection()
    {
        Assert.False(CreateRegistry().IsConnected("never-registered"));
    }

    [Fact]
    public async Task SendAsync_ReportsFailureForAnUnknownConnection()
    {
        var registry = CreateRegistry();

        var delivered = await registry.SendAsync(
            "never-registered",
            new SocketMessage { Type = SocketMessageTypes.JobCompleted, JobId = "job-1" });

        Assert.False(delivered);
    }

    [Fact]
    public void Remove_IsSafeForAnUnknownConnection()
    {
        var registry = CreateRegistry();

        registry.Remove("never-registered");

        Assert.Equal(0, registry.Count);
    }
}
