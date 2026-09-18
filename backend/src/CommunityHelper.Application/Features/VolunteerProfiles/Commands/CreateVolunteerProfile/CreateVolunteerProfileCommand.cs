using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.CreateVolunteerProfile;

public sealed record CreateVolunteerProfileCommand(
    List<string> Skills,
    string Availability,
    List<string> Causes,
    string Location,
    string Bio) : IRequest<VolunteerProfileDto>, IVolunteerProfileInput;
