using CommunityHelper.Application.Features.Opportunities.Queries.SearchOpportunities;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Opportunities;

public class SearchOpportunitiesQueryHandlerTests
{
    private static Opportunity MakeOpportunity(string id, DateTime startsAtUtc) =>
        Opportunity.Create(
            title: $"Opportunity {id}",
            description: "Description",
            location: "Dhaka",
            isRemote: false,
            startsAtUtc: startsAtUtc,
            id: id);

    [Fact]
    public async Task Handle_PassesTheSearchTermThroughToTheRepository()
    {
        var repository = new Mock<IOpportunityRepository>();
        repository
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await new SearchOpportunitiesQueryHandler(repository.Object)
            .Handle(new SearchOpportunitiesQuery("tutoring"), CancellationToken.None);

        repository.Verify(r => r.SearchAsync("tutoring", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSoonestMatchFirst()
    {
        var later = MakeOpportunity("later", new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc));
        var sooner = MakeOpportunity("sooner", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var repository = new Mock<IOpportunityRepository>();
        repository
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([later, sooner]);

        var result = await new SearchOpportunitiesQueryHandler(repository.Object)
            .Handle(new SearchOpportunitiesQuery("x"), CancellationToken.None);

        Assert.Equal(["sooner", "later"], result.Select(o => o.Id));
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenNothingMatches()
    {
        var repository = new Mock<IOpportunityRepository>();
        repository
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await new SearchOpportunitiesQueryHandler(repository.Object)
            .Handle(new SearchOpportunitiesQuery("nothing matches"), CancellationToken.None);

        Assert.Empty(result);
    }
}
