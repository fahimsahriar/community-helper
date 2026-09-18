using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResultDto>;
