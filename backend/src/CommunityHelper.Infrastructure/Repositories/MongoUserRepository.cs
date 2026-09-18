using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;
using CommunityHelper.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class MongoUserRepository(MongoDbContext context) : IUserRepository
{
    private IMongoCollection<UserDocument> Collection => context.Users;

    public async Task<User?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        if (!Persistence.DocumentMappers.IsObjectId(id))
        {
            return null;
        }

        var doc = await Collection
            .Find(Builders<UserDocument>.Filter.Eq(u => u.Id, id))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var doc = await Collection
            .Find(Builders<UserDocument>.Filter.Eq(
                u => u.Email, email.Trim().ToLowerInvariant()))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task<User?> FindByGoogleSubjectIdAsync(string subjectId, CancellationToken ct = default)
    {
        var doc = await Collection
            .Find(Builders<UserDocument>.Filter.Eq(u => u.GoogleSubjectId, subjectId))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task InsertAsync(User user, CancellationToken ct = default)
    {
        var doc = user.ToDocument();
        await Collection.InsertOneAsync(doc, cancellationToken: ct);
        user.SetId(doc.Id);
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        var result = await Collection.ReplaceOneAsync(
            Builders<UserDocument>.Filter.Eq(u => u.Id, user.Id),
            user.ToDocument(),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
