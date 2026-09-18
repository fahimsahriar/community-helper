using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer)
    : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Email", "Email is already registered."),
            });
        }

        var user = User.Create(
            request.Email,
            passwordHasher.Hash(request.Password),
            request.Role);

        await users.InsertAsync(user, cancellationToken);

        return await tokenIssuer.IssueAsync(user, cancellationToken);
    }
}
