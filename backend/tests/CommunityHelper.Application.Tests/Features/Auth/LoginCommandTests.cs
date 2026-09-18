using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Features.Auth;
using CommunityHelper.Application.Features.Auth.Commands.Login;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeJwtTokenService _tokens = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();

    private LoginCommandHandler Handler => new(
        _users.Object,
        _hasher,
        new AuthTokenIssuer(_tokens, _refreshTokens.Object));

    [Fact]
    public async Task Handle_ValidCredentials_IssuesTokenPair()
    {
        var user = User.Create("a@b.com", _hasher.Hash("password123"), UserRoles.Volunteer, "user-1");
        _users.Setup(r => r.FindByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await Handler.Handle(new LoginCommand("a@b.com", "password123"), CancellationToken.None);

        Assert.Equal("access-user-1", result.AccessToken);
        Assert.Equal("user-1", result.User.Id);
        Assert.Equal(UserRoles.Volunteer, result.User.Role);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsAuthenticationException()
    {
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Handler.Handle(new LoginCommand("nobody@x.com", "password123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsAuthenticationException()
    {
        var user = User.Create("a@b.com", _hasher.Hash("correct"), UserRoles.Volunteer, "user-1");
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Handler.Handle(new LoginCommand("a@b.com", "wrong"), CancellationToken.None));
    }
}
