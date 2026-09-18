using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Features.Auth.Commands.Logout;
using CommunityHelper.Application.Features.Auth.Commands.Refresh;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Auth;

public class RefreshCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly FakeJwtTokenService _tokens = new();

    private RefreshCommandHandler Handler => new(_users.Object, _refreshTokens.Object, _tokens);

    [Fact]
    public async Task Handle_ActiveToken_RotatesPair()
    {
        var user = User.Create("a@b.com", "hash", UserRoles.Volunteer, "user-1");
        var stored = RefreshToken.Create(_tokens.HashRefreshToken("refresh-old"), "user-1", DateTime.UtcNow.AddDays(7), "token-1");
        _refreshTokens.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _users.Setup(r => r.FindByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await Handler.Handle(new RefreshCommand("refresh-old"), CancellationToken.None);

        Assert.True(stored.IsRevoked);
        Assert.NotEqual("refresh-old", result.RefreshToken);
        Assert.Equal("access-user-1", result.AccessToken);
        _refreshTokens.Verify(r => r.UpdateAsync(stored, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(r => r.InsertAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsAuthenticationException()
    {
        _refreshTokens.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Handler.Handle(new RefreshCommand("bogus"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RevokedToken_RevokesFamilyAndThrows()
    {
        var stored = RefreshToken.Rehydrate(
            "token-1", _tokens.HashRefreshToken("refresh-old"), "user-1",
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6),
            DateTime.UtcNow.AddHours(-1), "HASH:refresh-next");
        _refreshTokens.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Handler.Handle(new RefreshCommand("refresh-old"), CancellationToken.None));

        _refreshTokens.Verify(r => r.RevokeAllForUserAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsAuthenticationException()
    {
        var stored = RefreshToken.Rehydrate(
            "token-1", _tokens.HashRefreshToken("refresh-old"), "user-1",
            DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-1),
            revokedAtUtc: null, replacedByTokenHash: null);
        _refreshTokens.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Handler.Handle(new RefreshCommand("refresh-old"), CancellationToken.None));
    }
}

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_KnownActiveToken_RevokesIt()
    {
        var repo = new Mock<IRefreshTokenRepository>();
        var tokens = new FakeJwtTokenService();
        var stored = RefreshToken.Create(tokens.HashRefreshToken("refresh-1"), "user-1", DateTime.UtcNow.AddDays(7), "token-1");
        repo.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        await new LogoutCommandHandler(repo.Object, tokens)
            .Handle(new LogoutCommand("refresh-1"), CancellationToken.None);

        Assert.True(stored.IsRevoked);
        repo.Verify(r => r.UpdateAsync(stored, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownToken_SucceedsSilently()
    {
        var repo = new Mock<IRefreshTokenRepository>();
        var tokens = new FakeJwtTokenService();
        repo.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await new LogoutCommandHandler(repo.Object, tokens)
            .Handle(new LogoutCommand("bogus"), CancellationToken.None);

        repo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
