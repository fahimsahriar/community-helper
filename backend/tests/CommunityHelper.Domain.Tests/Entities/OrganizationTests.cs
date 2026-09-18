using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Entities;

public class OrganizationTests
{
    private static Organization Create(
        string name = "Food Bank",
        string type = "Nonprofit",
        List<string>? causeTags = null,
        string location = "Dhaka",
        string description = "Feeding the city.") =>
        Organization.Create("owner-1", name, type, causeTags ?? ["Hunger"], location, description);

    [Fact]
    public void Create_StartsPendingVerification()
    {
        var organization = Create();

        Assert.Null(organization.VerifiedAtUtc);
    }

    [Fact]
    public void Create_RejectsBlankName()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(name: "  "));
        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Create_RejectsEmptyCauseTags()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(causeTags: []));
        Assert.Equal("causeTags", ex.ParamName);
    }

    [Fact]
    public void Update_ReplacesAllFields()
    {
        var organization = Create();

        organization.Update("New Name", "Informal group", ["Education"], "Sylhet", "New description.");

        Assert.Equal("New Name", organization.Name);
        Assert.Equal("Informal group", organization.Type);
        Assert.Equal(["Education"], organization.CauseTags);
        Assert.Equal("Sylhet", organization.Location);
        Assert.Equal("New description.", organization.Description);
        Assert.Null(organization.VerifiedAtUtc);
    }
}
