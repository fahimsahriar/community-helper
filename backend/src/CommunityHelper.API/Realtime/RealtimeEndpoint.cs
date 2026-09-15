using System.Net.WebSockets;
using CommunityHelper.Application.Common.Models;
using CommunityHelper.Infrastructure.Realtime;

namespace CommunityHelper.API.Realtime;

/// <summary>
/// The client's inbound half of the async request-reply pattern (docs/adr/0001).
/// Not a controller: this is a protocol upgrade holding a long-lived connection,
/// not a request/response action.
/// </summary>
public static class RealtimeEndpoint
{
    private const int ReceiveBufferSize = 4 * 1024;

    public static IEndpointRouteBuilder MapRealtimeEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/ws")
    {
        endpoints.Map(pattern, HandleAsync).ExcludeFromDescription();
        return endpoints;
    }

    private static async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("This endpoint requires a WebSocket upgrade.");
            return;
        }

        var registry = context.RequestServices.GetRequiredService<IWebSocketConnectionRegistry>();
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(RealtimeEndpoint));

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connection = registry.Add(socket);

        try
        {
            // The client cannot submit a job until it knows its connection id.
            await registry.SendAsync(
                connection.Id,
                new SocketMessage
                {
                    Type = SocketMessageTypes.ConnectionEstablished,
                    ConnectionId = connection.Id,
                },
                context.RequestAborted);

            await PumpUntilClosedAsync(connection, context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // Client went away or the host is shutting down; nothing to report.
        }
        catch (WebSocketException ex)
        {
            logger.LogWarning(ex, "Socket {ConnectionId} closed unexpectedly", connection.Id);
        }
        finally
        {
            registry.Remove(connection.Id);
        }
    }

    /// <summary>
    /// Holds the connection open. Clients send nothing today, but the receive
    /// loop is required: it is how a close frame is observed.
    /// </summary>
    private static async Task PumpUntilClosedAsync(WebSocketConnection connection, CancellationToken ct)
    {
        var buffer = new byte[ReceiveBufferSize];

        while (connection.IsOpen && !ct.IsCancellationRequested)
        {
            var result = await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await connection.Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    statusDescription: null,
                    ct);
                return;
            }

            // Inbound application messages are not part of the protocol yet.
        }
    }
}
