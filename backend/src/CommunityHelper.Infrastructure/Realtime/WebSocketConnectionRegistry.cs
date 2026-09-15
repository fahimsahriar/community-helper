using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using CommunityHelper.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CommunityHelper.Infrastructure.Realtime;

/// <summary>
/// In-process registry of live sockets. Single-instance only — see
/// docs/adr/0001 for what has to change to run more than one API replica.
/// </summary>
public sealed class WebSocketConnectionRegistry(ILogger<WebSocketConnectionRegistry> logger)
    : IWebSocketConnectionRegistry
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();

    public int Count => _connections.Count;

    public WebSocketConnection Add(WebSocket socket)
    {
        var connection = new WebSocketConnection(Guid.NewGuid().ToString("N"), socket);
        _connections[connection.Id] = connection;

        logger.LogInformation(
            "Socket {ConnectionId} connected ({ConnectionCount} open)",
            connection.Id,
            _connections.Count);

        return connection;
    }

    public void Remove(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            connection.Dispose();
            logger.LogInformation(
                "Socket {ConnectionId} disconnected ({ConnectionCount} open)",
                connectionId,
                _connections.Count);
        }
    }

    public bool IsConnected(string connectionId) =>
        _connections.TryGetValue(connectionId, out var connection) && connection.IsOpen;

    public async Task<bool> SendAsync(
        string connectionId,
        SocketMessage message,
        CancellationToken ct = default)
    {
        if (!_connections.TryGetValue(connectionId, out var connection) || !connection.IsOpen)
        {
            // Expected whenever a client navigates away mid-job.
            logger.LogWarning(
                "Dropped {MessageType} for {JobId}: connection {ConnectionId} is gone",
                message.Type,
                message.JobId,
                connectionId);
            return false;
        }

        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);
            await connection.SendAsync(payload, ct);
            return true;
        }
        catch (Exception ex) when (ex is WebSocketException or ObjectDisposedException or OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Failed to deliver {MessageType} to connection {ConnectionId}",
                message.Type,
                connectionId);
            return false;
        }
    }
}
