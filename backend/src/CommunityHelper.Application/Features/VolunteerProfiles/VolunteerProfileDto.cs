namespace CommunityHelper.Application.Features.VolunteerProfiles;

public sealed record VolunteerProfileDto(
    string UserId,
    List<string> Skills,
    string Availability,
    List<string> Causes,
    string Location,
    string Bio);
