using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUser,
    IUserRepository users)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        var user = await users.FindByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", currentUser.UserId);
        }

        return new CurrentUserDto(user.Id, user.Email, user.Role);
    }
}
