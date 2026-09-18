using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByGoogleSubjectIdAsync(string subjectId, CancellationToken ct = default);
    Task InsertAsync(User user, CancellationToken ct = default);
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);
}
