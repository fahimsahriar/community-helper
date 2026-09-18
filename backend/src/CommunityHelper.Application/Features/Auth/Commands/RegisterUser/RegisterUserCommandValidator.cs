using CommunityHelper.Domain.Entities;
using FluentValidation;

namespace CommunityHelper.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(UserRoles.IsKnown)
            .WithMessage("Role must be 'volunteer' or 'org_admin'.");
    }
}
