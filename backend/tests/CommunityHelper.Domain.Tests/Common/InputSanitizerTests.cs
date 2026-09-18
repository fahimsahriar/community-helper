using CommunityHelper.Domain.Common;
using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Common;

public class InputSanitizerTests
{
    [Fact]
    public void Clean_TrimsSurroundingWhitespace()
    {
        Assert.Equal("Dhaka", InputSanitizer.Clean("  Dhaka\t"));
    }

    [Fact]
    public void Clean_StripsNulAndControlCharacters()
    {
        Assert.Equal("abc", InputSanitizer.Clean("a\0b\u0007c"));
    }

    [Fact]
    public void Clean_StripsLogForgingNewlines()
    {
        Assert.Equal("line1line2", InputSanitizer.Clean("line1\r\nline2"));
    }

    [Fact]
    public void Clean_StripsBidiAndZeroWidthFormatCharacters()
    {
        Assert.Equal("abc", InputSanitizer.Clean("a\u200Bb\u202Ec"));
    }

    [Fact]
    public void Clean_PreservesLegitimateUnicodeAndMarkupAsText()
    {
        // Markup is kept verbatim for storage; output-encoding is the
        // consumers' job (Angular auto-escapes; Flutter Text is inert).
        Assert.Equal("ঢাকা <b>bold</b> & tea", InputSanitizer.Clean("  ঢাকা <b>bold</b> & tea "));
    }

    [Fact]
    public void CleanMultiline_PreservesNewlinesButStripsOtherControls()
    {
        Assert.Equal("line1\nline2", InputSanitizer.CleanMultiline("line1\nline2\0\u0007"));
    }

    [Fact]
    public void VolunteerProfile_SanitizesBioAndSkills()
    {
        var profile = VolunteerProfile.Create(
            "user-1",
            ["Teach\u200Bing"],
            "Week\u0007ends",
            ["Education"],
            "Dhaka",
            "Line1\nLine2\0");

        Assert.Equal(["Teaching"], profile.Skills);
        Assert.Equal("Weekends", profile.Availability);
        Assert.Equal("Line1\nLine2", profile.Bio);
    }

    [Fact]
    public void Organization_SanitizesNameAndDescription()
    {
        var org = Organization.Create(
            "user-1",
            "River\u202ECleaners",
            "Nonprofit",
            ["environment"],
            "Dhaka",
            "Cleaning\rrivers.");

        Assert.Equal("RiverCleaners", org.Name);
        Assert.Equal("Cleaning\rrivers.", org.Description);
    }

    [Fact]
    public void User_NormalizesEmail()
    {
        var user = User.Create("  VOL@Example.com ", "hash", "volunteer");

        Assert.Equal("vol@example.com", user.Email);
    }
}
