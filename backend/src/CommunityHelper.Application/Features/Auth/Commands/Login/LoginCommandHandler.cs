using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer)
    : IRequestHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken);

        // Same message either way so login cannot be used to enumerate accounts.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        return await tokenIssuer.IssueAsync(user, cancellationToken);
    }
}
