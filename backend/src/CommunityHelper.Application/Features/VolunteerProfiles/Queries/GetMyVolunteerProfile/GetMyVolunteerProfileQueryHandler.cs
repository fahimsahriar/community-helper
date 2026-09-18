using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Queries.GetMyVolunteerProfile;

public sealed class GetMyVolunteerProfileQueryHandler(
    ICurrentUserService currentUser,
    IVolunteerProfileRepository profiles)
    : IRequestHandler<GetMyVolunteerProfileQuery, VolunteerProfileDto>
{
    public async Task<VolunteerProfileDto> Handle(
        GetMyVolunteerProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.Volunteer))
        {
            throw new UnauthorizedAccessException("Only volunteers can view a volunteer profile.");
        }

        var profile = await profiles.FindByUserIdAsync(currentUser.UserId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("Volunteer profile", currentUser.UserId);
        }

        return new VolunteerProfileDto(
            profile.UserId,
            profile.Skills,
            profile.Availability,
            profile.Causes,
            profile.Location,
            profile.Bio);
    }
}
