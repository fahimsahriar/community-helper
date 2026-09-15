using CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Queries.SearchOpportunities;

public class SearchOpportunitiesQueryHandler(IOpportunityRepository opportunities)
    : IRequestHandler<SearchOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    public async Task<IReadOnlyList<OpportunityDto>> Handle(
        SearchOpportunitiesQuery request,
        CancellationToken cancellationToken)
    {
        var results = await opportunities.SearchAsync(request.Query, cancellationToken);

        return results
            .OrderBy(o => o.StartsAtUtc)
            .Select(o => new OpportunityDto(
                o.Id,
                o.Title,
                o.Description,
                o.Location,
                o.IsRemote,
                o.StartsAtUtc))
            .ToList();
    }
}
