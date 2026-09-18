using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
