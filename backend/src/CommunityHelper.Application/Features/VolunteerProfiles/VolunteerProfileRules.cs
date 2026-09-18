using FluentValidation;

namespace CommunityHelper.Application.Features.VolunteerProfiles;

internal static class VolunteerProfileRules
{
    public static void ApplyProfileRules<T>(AbstractValidator<T> validator)
        where T : IVolunteerProfileInput
    {
        validator.RuleFor(x => x.Skills)
            .NotNull()
            .Must(s => s.Count > 0 && s.Count <= 20)
            .WithMessage("Provide between 1 and 20 skills.")
            .ForEach(skill => skill.NotEmpty().MaximumLength(100));

        validator.RuleFor(x => x.Availability)
            .NotEmpty()
            .MaximumLength(200);

        validator.RuleFor(x => x.Causes)
            .NotNull()
            .Must(c => c.Count > 0 && c.Count <= 20)
            .WithMessage("Provide between 1 and 20 causes.")
            .ForEach(cause => cause.NotEmpty().MaximumLength(100));

        validator.RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        validator.RuleFor(x => x.Bio)
            .NotEmpty()
            .MaximumLength(2000);
    }
}

public interface IVolunteerProfileInput
{
    List<string> Skills { get; }
    string Availability { get; }
    List<string> Causes { get; }
    string Location { get; }
    string Bio { get; }
}
