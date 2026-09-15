using CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;
using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Queries.SearchOpportunities;

/// <summary>
/// The actual work behind an opportunity-search job. Runs on the background
/// worker, not on the HTTP request thread — see docs/adr/0001.
/// </summary>
public record SearchOpportunitiesQuery(string Query) : IRequest<IReadOnlyList<OpportunityDto>>;
