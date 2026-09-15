using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;

public record GetOpportunitiesQuery : IRequest<IReadOnlyList<OpportunityDto>>;
