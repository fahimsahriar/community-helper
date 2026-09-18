using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_NormalizesEmail()
    {
        var user = User.Create("  VOLUNTEER@Example.COM ", "hash", UserRoles.Volunteer);

        Assert.Equal("volunteer@example.com", user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankEmail(string email)
    {
        var ex = Assert.Throws<ArgumentException>(() => User.Create(email, "hash", UserRoles.Volunteer));
        Assert.Equal("email", ex.ParamName);
    }

    [Fact]
    public void Create_RejectsBlankPasswordHash()
    {
        var ex = Assert.Throws<ArgumentException>(() => User.Create("a@b.com", "  ", UserRoles.Volunteer));
        Assert.Equal("passwordHash", ex.ParamName);
    }

    [Theory]
    [InlineData("corp_admin")]
    [InlineData("platform_admin")]
    [InlineData("superuser")]
    [InlineData("")]
    public void Create_RejectsUnknownRole(string role)
    {
        var ex = Assert.Throws<ArgumentException>(() => User.Create("a@b.com", "hash", role));
        Assert.Equal("role", ex.ParamName);
    }

    [Theory]
    [InlineData(UserRoles.Volunteer)]
    [InlineData(UserRoles.OrgAdmin)]
    public void Create_AcceptsKnownRoles(string role)
    {
        var user = User.Create("a@b.com", "hash", role);

        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void LinkGoogleSubject_AssignsSubject()
    {
        var user = User.Create("a@b.com", "hash", UserRoles.Volunteer);

        user.LinkGoogleSubject("google-123");

        Assert.Equal("google-123", user.GoogleSubjectId);
    }

    [Fact]
    public void IsKnown_ReturnsFalseForDeferredRoles()
    {
        Assert.False(UserRoles.IsKnown("corp_admin"));
        Assert.False(UserRoles.IsKnown(null));
        Assert.True(UserRoles.IsKnown(UserRoles.Volunteer));
        Assert.True(UserRoles.IsKnown(UserRoles.OrgAdmin));
    }
}
