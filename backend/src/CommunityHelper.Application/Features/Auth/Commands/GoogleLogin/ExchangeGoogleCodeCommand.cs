using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.GoogleLogin;

public sealed record ExchangeGoogleCodeCommand(string Code, string Role) : IRequest<AuthResultDto>;
