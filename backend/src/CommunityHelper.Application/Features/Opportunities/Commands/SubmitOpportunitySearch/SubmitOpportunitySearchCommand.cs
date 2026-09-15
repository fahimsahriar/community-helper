using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Commands.SubmitOpportunitySearch;

/// <summary>
/// Queues an opportunity search and returns its job id. Returns immediately —
/// the result arrives on the client's socket connection.
/// </summary>
/// <returns>The job id the client correlates socket messages with.</returns>
public record SubmitOpportunitySearchCommand(string ConnectionId, string Query) : IRequest<string>;
