using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Interfaces;

public interface IVolunteerProfileRepository
{
    Task<VolunteerProfile?> FindByUserIdAsync(string userId, CancellationToken ct = default);
    Task InsertAsync(VolunteerProfile profile, CancellationToken ct = default);
    Task<bool> UpdateAsync(VolunteerProfile profile, CancellationToken ct = default);
}
