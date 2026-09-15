using MediatR;

namespace CommunityHelper.Application.Common.Models;

/// <summary>
/// A unit of background work: any MediatR request, plus the routing information
/// needed to push its result back to the client that asked for it.
/// </summary>
/// <param name="JobId">Server-generated correlation id returned to the client.</param>
/// <param name="ConnectionId">Socket connection that submitted the job.</param>
/// <param name="Request">Dispatched via <c>ISender.Send(object)</c> by the worker.</param>
public sealed record JobWorkItem(string JobId, string ConnectionId, IBaseRequest Request);
