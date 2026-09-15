using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CommunityHelper.API.IntegrationTests;

/// <summary>
/// Covers the full async request-reply loop described in docs/adr/0001:
/// socket handshake, HTTP submit, and the pushed result.
/// </summary>
public class AsyncJobFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    private async Task<(WebSocket Socket, string ConnectionId)> ConnectAsync(CancellationToken ct)
    {
        var socket = await factory.Server
            .CreateWebSocketClient()
            .ConnectAsync(new Uri(factory.Server.BaseAddress, "ws"), ct);

        var handshake = await SocketTestClient.ReceiveOfTypeAsync(socket, "connection.established", ct);
        return (socket, handshake.GetProperty("connectionId").GetString()!);
    }

    [Fact]
    public async Task Handshake_AssignsAConnectionId()
    {
        using var cts = new CancellationTokenSource(Timeout);

        var (socket, connectionId) = await ConnectAsync(cts.Token);
        using (socket)
        {
            Assert.False(string.IsNullOrWhiteSpace(connectionId));
        }
    }

    [Fact]
    public async Task SubmittedJob_IsAcceptedOverHttpAndCompletedOverTheSocket()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = factory.CreateClient();

        var (socket, connectionId) = await ConnectAsync(cts.Token);
        using (socket)
        {
            var response = await client.PostAsJsonAsync(
                "/api/jobs/opportunity-search",
                new { connectionId, query = "tutoring" },
                cts.Token);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var accepted = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
            var jobId = accepted.GetProperty("jobId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(jobId));

            var completed = await SocketTestClient.ReceiveOfTypeAsync(socket, "job.completed", cts.Token);

            Assert.Equal(jobId, completed.GetProperty("jobId").GetString());

            var results = completed.GetProperty("result").EnumerateArray().ToList();
            var single = Assert.Single(results);
            Assert.Contains("Tutoring", single.GetProperty("title").GetString());
        }
    }

    [Fact]
    public async Task SubmittedJob_AnnouncesItIsRunningBeforeItCompletes()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = factory.CreateClient();

        var (socket, connectionId) = await ConnectAsync(cts.Token);
        using (socket)
        {
            await client.PostAsJsonAsync(
                "/api/jobs/opportunity-search",
                new { connectionId, query = "" },
                cts.Token);

            var first = await SocketTestClient.ReceiveAsync(socket, cts.Token);

            Assert.Equal("job.running", first.GetProperty("type").GetString());
        }
    }

    [Fact]
    public async Task SubmittedJob_WithAnUnknownConnectionIsRejectedWithAnErrorsMap()
    {
        // Regression: ValidationProblemDetails.Errors was dropped when the
        // response was serialized through its ProblemDetails base type.
        using var cts = new CancellationTokenSource(Timeout);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/jobs/opportunity-search",
            new { connectionId = "not-a-real-connection", query = "x" },
            cts.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        var errors = problem.GetProperty("errors");
        var connectionErrors = errors.GetProperty("ConnectionId").EnumerateArray().ToList();

        Assert.NotEmpty(connectionErrors);
        Assert.Contains("Reconnect", connectionErrors[0].GetString());
    }

    [Fact]
    public async Task RealtimeEndpoint_RejectsAPlainHttpRequest()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/ws", cts.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOpportunities_ReturnsTheSeededList()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/opportunities", cts.Token);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);

        Assert.Equal(3, body.EnumerateArray().Count());
    }
}
