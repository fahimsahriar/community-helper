namespace CommunityHelper.Application.Features.Organizations;

public sealed record OrganizationDto(
    string Id,
    string OwnerUserId,
    string Name,
    string Type,
    List<string> CauseTags,
    string Location,
    string Description,
    DateTime? VerifiedAtUtc,
    bool IsVerified);
