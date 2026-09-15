using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Entities;

public class OpportunityTests
{
    private static Opportunity Create(
        string title = "Food Bank Sorting",
        string description = "Sort donated goods.",
        string location = "Dhaka",
        bool isRemote = false) =>
        Opportunity.Create(title, description, location, isRemote, DateTime.UtcNow);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankTitle(string title)
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(title: title));
        Assert.Equal("title", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankDescription(string description)
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(description: description));
        Assert.Equal("description", ex.ParamName);
    }

    [Fact]
    public void Create_RejectsOnSiteOpportunityWithoutLocation()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create(location: "  ", isRemote: false));
        Assert.Equal("location", ex.ParamName);
    }

    [Fact]
    public void Create_AllowsRemoteOpportunityWithoutLocation()
    {
        var opportunity = Create(location: "", isRemote: true);

        Assert.True(opportunity.IsRemote);
        Assert.Equal(string.Empty, opportunity.Location);
    }

    [Fact]
    public void Create_TrimsWhitespaceFromTextFields()
    {
        var opportunity = Create(title: "  Tutoring  ", description: "  Teach maths.  ", location: "  Chattogram  ");

        Assert.Equal("Tutoring", opportunity.Title);
        Assert.Equal("Teach maths.", opportunity.Description);
        Assert.Equal("Chattogram", opportunity.Location);
    }

    [Fact]
    public void SetId_AssignsPersistenceGeneratedIdentifier()
    {
        var opportunity = Create();
        Assert.Equal(string.Empty, opportunity.Id);

        opportunity.SetId("507f1f77bcf86cd799439011");

        Assert.Equal("507f1f77bcf86cd799439011", opportunity.Id);
    }

    [Fact]
    public void SetId_RejectsBlankIdentifier()
    {
        var ex = Assert.Throws<ArgumentException>(() => Create().SetId("  "));
        Assert.Equal("id", ex.ParamName);
    }
}
