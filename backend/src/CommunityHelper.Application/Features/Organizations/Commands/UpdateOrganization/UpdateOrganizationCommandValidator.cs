using FluentValidation;

namespace CommunityHelper.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        OrganizationRules.ApplyOrganizationRules(this);
    }
}
