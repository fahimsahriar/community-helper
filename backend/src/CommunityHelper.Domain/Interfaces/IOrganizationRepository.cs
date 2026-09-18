using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<Organization?> FindByOwnerUserIdAsync(string ownerUserId, CancellationToken ct = default);
    Task InsertAsync(Organization organization, CancellationToken ct = default);
    Task<bool> UpdateAsync(Organization organization, CancellationToken ct = default);
}
