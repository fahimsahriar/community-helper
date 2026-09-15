using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CommunityHelper.API.IntegrationTests;

/// <summary>Reads one JSON envelope at a time off a test WebSocket.</summary>
public static class SocketTestClient
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static async Task<JsonElement> ReceiveAsync(WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[16 * 1024];
        var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

        var json = Encoding.UTF8.GetString(buffer, 0, received.Count);
        return JsonSerializer.Deserialize<JsonElement>(json, Options);
    }

    /// <summary>Reads until a message of the given type arrives, or the token trips.</summary>
    public static async Task<JsonElement> ReceiveOfTypeAsync(
        WebSocket socket,
        string type,
        CancellationToken ct)
    {
        while (true)
        {
            var message = await ReceiveAsync(socket, ct);
            if (message.GetProperty("type").GetString() == type)
            {
                return message;
            }
        }
    }
}
