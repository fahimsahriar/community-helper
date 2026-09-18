using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Entities;

public class VolunteerProfileTests
{
    private static VolunteerProfile Create(
        List<string>? skills = null,
        string availability = "Weekends",
        List<string>? causes = null,
        string location = "Dhaka",
        string bio = "Happy to help.") =>
        VolunteerProfile.Create(
            "user-1",
            skills ?? ["Teaching"],
            availability,
            causes ?? ["Education"],
            location,
            bio);

    [Fact]
    public void Create_RejectsBlankUserId()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            VolunteerProfile.Create("  ", ["Teaching"], "Weekends", ["Education"], "Dhaka", "Bio"));
        Assert.Equal("userId", ex.ParamName);
    }

    [Fact]
    public void Create_RejectsEmptySkills()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(skills: []));
        Assert.Equal("skills", ex.ParamName);
    }

    [Fact]
    public void Create_RejectsEmptyCauses()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(causes: ["  ", ""]));
        Assert.Equal("causes", ex.ParamName);
    }

    [Fact]
    public void Update_ReplacesAllFields()
    {
        var profile = Create();

        profile.Update(["Cooking", "Driving"], "Evenings", ["Hunger"], "Chattogram", "New bio.");

        Assert.Equal(["Cooking", "Driving"], profile.Skills);
        Assert.Equal("Evenings", profile.Availability);
        Assert.Equal(["Hunger"], profile.Causes);
        Assert.Equal("Chattogram", profile.Location);
        Assert.Equal("New bio.", profile.Bio);
    }
}
