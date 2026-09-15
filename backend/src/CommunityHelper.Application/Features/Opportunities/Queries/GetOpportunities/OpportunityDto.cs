namespace CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;

public record OpportunityDto(
    string Id,
    string Title,
    string Description,
    string Location,
    bool IsRemote,
    DateTime StartsAtUtc);
