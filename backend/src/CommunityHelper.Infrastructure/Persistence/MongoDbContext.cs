using CommunityHelper.Infrastructure.Options;
using CommunityHelper.Infrastructure.Persistence.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CommunityHelper.Infrastructure.Persistence;

/// <summary>
/// Collection accessors for the MongoDB database. Registered only when a
/// <c>MongoDb:ConnectionString</c> is configured; otherwise the in-memory
/// repositories are used (local dev and tests).
/// </summary>
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<UserDocument> Users =>
        _database.GetCollection<UserDocument>("users");

    public IMongoCollection<VolunteerProfileDocument> VolunteerProfiles =>
        _database.GetCollection<VolunteerProfileDocument>("volunteer_profiles");

    public IMongoCollection<OrganizationDocument> Organizations =>
        _database.GetCollection<OrganizationDocument>("organizations");

    public IMongoCollection<RefreshTokenDocument> RefreshTokens =>
        _database.GetCollection<RefreshTokenDocument>("refresh_tokens");

    /// <summary>
    /// Creates the indexes the repositories rely on: unique email, cause/location
    /// lookups, and refresh-token hash lookup.
    /// </summary>
    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        await Users.Indexes.CreateOneAsync(
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        await Users.Indexes.CreateOneAsync(
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(u => u.GoogleSubjectId),
                new CreateIndexOptions { Sparse = true }),
            cancellationToken: cancellationToken);

        await VolunteerProfiles.Indexes.CreateOneAsync(
            new CreateIndexModel<VolunteerProfileDocument>(
                Builders<VolunteerProfileDocument>.IndexKeys.Ascending(p => p.UserId),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        await Organizations.Indexes.CreateOneAsync(
            new CreateIndexModel<OrganizationDocument>(
                Builders<OrganizationDocument>.IndexKeys.Ascending(o => o.OwnerUserId)),
            cancellationToken: cancellationToken);

        await Organizations.Indexes.CreateOneAsync(
            new CreateIndexModel<OrganizationDocument>(
                Builders<OrganizationDocument>.IndexKeys
                    .Ascending(o => o.CauseTags)
                    .Ascending(o => o.Location)),
            cancellationToken: cancellationToken);

        await RefreshTokens.Indexes.CreateOneAsync(
            new CreateIndexModel<RefreshTokenDocument>(
                Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.TokenHash),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        await RefreshTokens.Indexes.CreateOneAsync(
            new CreateIndexModel<RefreshTokenDocument>(
                Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.UserId)),
            cancellationToken: cancellationToken);
    }
}
