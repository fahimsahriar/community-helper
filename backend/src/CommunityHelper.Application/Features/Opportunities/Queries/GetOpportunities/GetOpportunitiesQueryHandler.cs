using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;

public class GetOpportunitiesQueryHandler(IOpportunityRepository opportunities)
    : IRequestHandler<GetOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    public async Task<IReadOnlyList<OpportunityDto>> Handle(
        GetOpportunitiesQuery request,
        CancellationToken cancellationToken)
    {
        var results = await opportunities.GetAllAsync(cancellationToken);

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
