using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Features.Auth;
using CommunityHelper.Application.Features.Auth.Commands.GoogleLogin;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Auth;

public class ExchangeGoogleCodeCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly Mock<IGoogleAuthService> _google = new();
    private readonly FakeJwtTokenService _tokens = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();

    private ExchangeGoogleCodeCommandHandler Handler => new(
        _users.Object,
        _hasher,
        _google.Object,
        new AuthTokenIssuer(_tokens, _refreshTokens.Object));

    [Fact]
    public async Task Handle_NewGoogleUser_ProvisionsVolunteerAndIssuesTokens()
    {
        _google.Setup(g => g.ExchangeCodeAsync("code-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("google-1", "New@Example.com"));
        _users.Setup(r => r.FindByGoogleSubjectIdAsync("google-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? inserted = null;
        _users.Setup(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) =>
            {
                // Real repositories propagate the generated id back via SetId.
                u.SetId("user-9");
                inserted = u;
            })
            .Returns(Task.CompletedTask);

        var result = await Handler.Handle(
            new ExchangeGoogleCodeCommand("code-1", UserRoles.Volunteer),
            CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Equal("new@example.com", inserted.Email);
        Assert.Equal("google-1", inserted.GoogleSubjectId);
        Assert.Equal(UserRoles.Volunteer, result.User.Role);
    }

    [Fact]
    public async Task Handle_ExistingEmail_LinksGoogleSubject()
    {
        var user = User.Create("a@b.com", "hash", UserRoles.Volunteer, "user-1");
        _google.Setup(g => g.ExchangeCodeAsync("code-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("google-1", "a@b.com"));
        _users.Setup(r => r.FindByGoogleSubjectIdAsync("google-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Handler.Handle(new ExchangeGoogleCodeCommand("code-1", UserRoles.Volunteer), CancellationToken.None);

        Assert.Equal("google-1", user.GoogleSubjectId);
        _users.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
