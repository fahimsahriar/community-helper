using FluentValidation;

namespace CommunityHelper.Application.Features.VolunteerProfiles.Commands.CreateVolunteerProfile;

public sealed class CreateVolunteerProfileCommandValidator : AbstractValidator<CreateVolunteerProfileCommand>
{
    public CreateVolunteerProfileCommandValidator()
    {
        VolunteerProfileRules.ApplyProfileRules(this);
    }
}
