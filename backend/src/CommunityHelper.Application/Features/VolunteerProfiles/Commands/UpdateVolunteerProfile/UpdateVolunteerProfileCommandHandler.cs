using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.UpdateVolunteerProfile;

public sealed class UpdateVolunteerProfileCommandHandler(
    ICurrentUserService currentUser,
    IVolunteerProfileRepository profiles)
    : IRequestHandler<UpdateVolunteerProfileCommand, VolunteerProfileDto>
{
    public async Task<VolunteerProfileDto> Handle(
        UpdateVolunteerProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.Volunteer))
        {
            throw new UnauthorizedAccessException("Only volunteers can edit a volunteer profile.");
        }

        var profile = await profiles.FindByUserIdAsync(currentUser.UserId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("Volunteer profile", currentUser.UserId);
        }

        profile.Update(
            request.Skills,
            request.Availability,
            request.Causes,
            request.Location,
            request.Bio);

        await profiles.UpdateAsync(profile, cancellationToken);

        return new VolunteerProfileDto(
            profile.UserId,
            profile.Skills,
            profile.Availability,
            profile.Causes,
            profile.Location,
            profile.Bio);
    }
}
