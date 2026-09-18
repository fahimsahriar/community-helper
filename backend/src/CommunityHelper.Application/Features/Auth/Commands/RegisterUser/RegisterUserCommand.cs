using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string Role) : IRequest<AuthResultDto>;
