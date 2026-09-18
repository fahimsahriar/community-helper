using FluentValidation;

namespace CommunityHelper.Application.Features.Organizations.Commands.RegisterOrganization;

public sealed class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationCommandValidator()
    {
        OrganizationRules.ApplyOrganizationRules(this);
    }
}
