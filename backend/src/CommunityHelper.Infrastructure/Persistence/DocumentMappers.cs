using CommunityHelper.Domain.Entities;
using CommunityHelper.Infrastructure.Persistence.Documents;
using MongoDB.Bson;

namespace CommunityHelper.Infrastructure.Persistence;

/// <summary>
/// Maps between domain entities and MongoDB documents. The Application and
/// Domain layers never see driver types.
/// </summary>
internal static class DocumentMappers
{
    public static string NewId() => ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// Guards id filters: malformed ids match nothing instead of throwing.
    /// </summary>
    public static bool IsObjectId(string? id) => ObjectId.TryParse(id, out _);

    public static UserDocument ToDocument(this User user) => new()
    {
        Id = string.IsNullOrEmpty(user.Id) ? NewId() : user.Id,
        Email = user.Email,
        PasswordHash = user.PasswordHash,
        Role = user.Role,
        CreatedAtUtc = user.CreatedAtUtc,
        GoogleSubjectId = user.GoogleSubjectId,
    };

    public static User ToDomain(this UserDocument doc) =>
        User.Rehydrate(doc.Id, doc.Email, doc.PasswordHash, doc.Role, doc.CreatedAtUtc, doc.GoogleSubjectId);

    public static VolunteerProfileDocument ToDocument(this VolunteerProfile profile) => new()
    {
        Id = string.IsNullOrEmpty(profile.Id) ? NewId() : profile.Id,
        UserId = profile.UserId,
        Skills = profile.Skills,
        Availability = profile.Availability,
        Causes = profile.Causes,
        Location = profile.Location,
        Bio = profile.Bio,
    };

    public static VolunteerProfile ToDomain(this VolunteerProfileDocument doc)
    {
        var profile = VolunteerProfile.Create(
            doc.UserId,
            doc.Skills,
            doc.Availability,
            doc.Causes,
            doc.Location,
            doc.Bio,
            doc.Id);
        return profile;
    }

    public static OrganizationDocument ToDocument(this Organization organization) => new()
    {
        Id = string.IsNullOrEmpty(organization.Id) ? NewId() : organization.Id,
        OwnerUserId = organization.OwnerUserId,
        Name = organization.Name,
        Type = organization.Type,
        CauseTags = organization.CauseTags,
        Location = organization.Location,
        Description = organization.Description,
        VerifiedAtUtc = organization.VerifiedAtUtc,
    };

    public static Organization ToDomain(this OrganizationDocument doc) =>
        Organization.Rehydrate(
            doc.Id,
            doc.OwnerUserId,
            doc.Name,
            doc.Type,
            doc.CauseTags,
            doc.Location,
            doc.Description,
            doc.VerifiedAtUtc);

    public static RefreshTokenDocument ToDocument(this RefreshToken token) => new()
    {
        Id = string.IsNullOrEmpty(token.Id) ? NewId() : token.Id,
        TokenHash = token.TokenHash,
        UserId = token.UserId,
        CreatedAtUtc = token.CreatedAtUtc,
        ExpiresAtUtc = token.ExpiresAtUtc,
        RevokedAtUtc = token.RevokedAtUtc,
        ReplacedByTokenHash = token.ReplacedByTokenHash,
    };

    public static RefreshToken ToDomain(this RefreshTokenDocument doc) =>
        RefreshToken.Rehydrate(
            doc.Id,
            doc.TokenHash,
            doc.UserId,
            doc.CreatedAtUtc,
            doc.ExpiresAtUtc,
            doc.RevokedAtUtc,
            doc.ReplacedByTokenHash);
}
