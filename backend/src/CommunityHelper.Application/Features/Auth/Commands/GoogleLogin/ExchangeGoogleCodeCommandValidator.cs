using CommunityHelper.Domain.Entities;
using FluentValidation;

namespace CommunityHelper.Application.Features.Auth.Commands.GoogleLogin;

public sealed class ExchangeGoogleCodeCommandValidator : AbstractValidator<ExchangeGoogleCodeCommand>
{
    public ExchangeGoogleCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(4096);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(UserRoles.IsKnown)
            .WithMessage("Role must be 'volunteer' or 'org_admin'.");
    }
}
