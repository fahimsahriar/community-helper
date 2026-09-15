using System.Text.Json.Serialization;

namespace CommunityHelper.Application.Common.Models;

/// <summary>
/// The single envelope every server-to-client socket message uses.
/// Clients discriminate on <see cref="Type"/>; see docs/adr/0001.
/// </summary>
public sealed record SocketMessage
{
    public required string Type { get; init; }

    /// <summary>Set only on <see cref="SocketMessageTypes.ConnectionEstablished"/>.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConnectionId { get; init; }

    /// <summary>Correlates a message with the job the client submitted.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JobId { get; init; }

    /// <summary>The handler's return value. Present on `job.completed` only.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; init; }

    /// <summary>Human-readable failure reason. Present on `job.failed` only.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }
}

public static class SocketMessageTypes
{
    public const string ConnectionEstablished = "connection.established";
    public const string JobRunning = "job.running";
    public const string JobCompleted = "job.completed";
    public const string JobFailed = "job.failed";
}
