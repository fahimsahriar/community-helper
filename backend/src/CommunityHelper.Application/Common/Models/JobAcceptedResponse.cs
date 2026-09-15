namespace CommunityHelper.Application.Common.Models;

/// <summary>
/// Returned by every job submission endpoint. The client correlates subsequent
/// socket messages with <paramref name="JobId"/>.
/// </summary>
public record JobAcceptedResponse(string JobId);
