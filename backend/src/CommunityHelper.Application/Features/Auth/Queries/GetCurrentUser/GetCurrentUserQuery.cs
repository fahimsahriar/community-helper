using MediatR;

namespace CommunityHelper.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;
