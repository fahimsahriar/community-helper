using CommunityHelper.Application.Features.Auth;
using CommunityHelper.Application.Features.Auth.Commands.RegisterUser;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeJwtTokenService _tokens = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();

    private RegisterUserCommandHandler Handler => new(
        _users.Object,
        _hasher,
        new AuthTokenIssuer(_tokens, _refreshTokens.Object));

    [Fact]
    public async Task Handle_NewEmail_InsertsUserAndIssuesTokenPair()
    {
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? inserted = null;
        _users.Setup(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) =>
            {
                // Real repositories propagate the generated id back via SetId.
                u.SetId("user-1");
                inserted = u;
            })
            .Returns(Task.CompletedTask);

        var result = await Handler.Handle(
            new RegisterUserCommand("New@Example.com", "password123", UserRoles.Volunteer),
            CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Equal("new@example.com", inserted.Email);
        Assert.Equal($"HASHED:password123", inserted.PasswordHash);
        Assert.NotEqual(result.AccessToken, result.RefreshToken);
        Assert.Contains("access-", result.AccessToken);
        _refreshTokens.Verify(
            r => r.InsertAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsValidationException()
    {
        var existing = User.Create("a@b.com", "hash", UserRoles.Volunteer, "user-1");
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<ValidationException>(() => Handler.Handle(
            new RegisterUserCommand("a@b.com", "password123", UserRoles.Volunteer),
            CancellationToken.None));
    }

    [Theory]
    [InlineData("not-an-email", "password123", "volunteer")]
    [InlineData("a@b.com", "short", "volunteer")]
    [InlineData("a@b.com", "password123", "corp_admin")]
    [InlineData("a@b.com", "password123", "")]
    public void Validator_RejectsBadInput(string email, string password, string role)
    {
        var validator = new RegisterUserCommandValidator();

        var result = validator.Validate(new RegisterUserCommand(email, password, role));

        Assert.False(result.IsValid);
    }
}
