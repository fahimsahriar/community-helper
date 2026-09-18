using CommunityHelper.Domain.Entities;
using FluentValidation;

namespace CommunityHelper.Application.Features.Organizations;

internal static class OrganizationRules
{
    public static void ApplyOrganizationRules<T>(AbstractValidator<T> validator)
        where T : IOrganizationInput
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        validator.RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        validator.RuleFor(x => x.CauseTags)
            .NotNull()
            .Must(t => t.Count > 0 && t.Count <= 20)
            .WithMessage("Provide between 1 and 20 cause tags.")
            .ForEach(tag => tag.NotEmpty().MaximumLength(100));

        validator.RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        validator.RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);
    }

    public static OrganizationDto ToDto(Organization organization) => new(
        organization.Id,
        organization.OwnerUserId,
        organization.Name,
        organization.Type,
        organization.CauseTags,
        organization.Location,
        organization.Description,
        organization.VerifiedAtUtc,
        organization.VerifiedAtUtc.HasValue);
}

public interface IOrganizationInput
{
    string Name { get; }
    string Type { get; }
    List<string> CauseTags { get; }
    string Location { get; }
    string Description { get; }
}
