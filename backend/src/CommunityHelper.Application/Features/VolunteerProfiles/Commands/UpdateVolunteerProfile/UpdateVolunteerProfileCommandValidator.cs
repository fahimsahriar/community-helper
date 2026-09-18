using FluentValidation;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.UpdateVolunteerProfile;

public sealed class UpdateVolunteerProfileCommandValidator : AbstractValidator<UpdateVolunteerProfileCommand>
{
    public UpdateVolunteerProfileCommandValidator()
    {
        VolunteerProfileRules.ApplyProfileRules(this);
    }
}
