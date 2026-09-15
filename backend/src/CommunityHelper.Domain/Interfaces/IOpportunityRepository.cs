using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Interfaces;

public interface IOpportunityRepository
{
    Task<IReadOnlyList<Opportunity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Free-text match over title, description and location.
    /// A blank query matches everything.
    /// </summary>
    Task<IReadOnlyList<Opportunity>> SearchAsync(string query, CancellationToken ct = default);
}
