using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Queries.GetMyVolunteerProfile;

public sealed record GetMyVolunteerProfileQuery : IRequest<VolunteerProfileDto>;
