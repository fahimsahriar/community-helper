using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.UpdateVolunteerProfile;

public sealed record UpdateVolunteerProfileCommand(
    List<string> Skills,
    string Availability,
    List<string> Causes,
    string Location,
    string Bio) : IRequest<VolunteerProfileDto>, IVolunteerProfileInput;
