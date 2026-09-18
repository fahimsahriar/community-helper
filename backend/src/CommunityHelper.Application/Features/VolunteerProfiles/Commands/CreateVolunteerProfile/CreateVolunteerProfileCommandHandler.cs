using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.CreateVolunteerProfile;

public sealed class CreateVolunteerProfileCommandHandler(
    ICurrentUserService currentUser,
    IVolunteerProfileRepository profiles)
    : IRequestHandler<CreateVolunteerProfileCommand, VolunteerProfileDto>
{
    public async Task<VolunteerProfileDto> Handle(
        CreateVolunteerProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.Volunteer))
        {
            throw new UnauthorizedAccessException("Only volunteers can create a volunteer profile.");
        }

        var existing = await profiles.FindByUserIdAsync(currentUser.UserId, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Profile", "Volunteer profile already exists. Use update instead."),
            });
        }

        var profile = VolunteerProfile.Create(
            currentUser.UserId,
            request.Skills,
            request.Availability,
            request.Causes,
            request.Location,
            request.Bio);

        await profiles.InsertAsync(profile, cancellationToken);

        return Map(profile);
    }

    internal static VolunteerProfileDto Map(VolunteerProfile profile) => new(
        profile.UserId,
        profile.Skills,
        profile.Availability,
        profile.Causes,
        profile.Location,
        profile.Bio);
}
