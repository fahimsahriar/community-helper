using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.GoogleLogin;

public sealed class ExchangeGoogleCodeCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IGoogleAuthService google,
    AuthTokenIssuer tokenIssuer)
    : IRequestHandler<ExchangeGoogleCodeCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(ExchangeGoogleCodeCommand request, CancellationToken cancellationToken)
    {
        var info = await google.ExchangeCodeAsync(request.Code, cancellationToken);

        var user = await users.FindByGoogleSubjectIdAsync(info.SubjectId, cancellationToken);
        if (user is not null)
        {
            return await tokenIssuer.IssueAsync(user, cancellationToken);
        }

        user = await users.FindByEmailAsync(info.Email, cancellationToken);
        if (user is not null)
        {
            user.LinkGoogleSubject(info.SubjectId);
            await users.UpdateAsync(user, cancellationToken);
            return await tokenIssuer.IssueAsync(user, cancellationToken);
        }

        // First Google sign-in: provision a user with an unusable random password.
        var provisioned = User.Create(
            info.Email,
            passwordHasher.Hash(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")),
            request.Role,
            googleSubjectId: info.SubjectId);

        await users.InsertAsync(provisioned, cancellationToken);

        return await tokenIssuer.IssueAsync(provisioned, cancellationToken);
    }
}
