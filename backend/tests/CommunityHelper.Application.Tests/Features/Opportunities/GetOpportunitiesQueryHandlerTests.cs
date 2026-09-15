using CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Opportunities;

public class GetOpportunitiesQueryHandlerTests
{
    private static Opportunity MakeOpportunity(string id, string title, DateTime startsAtUtc) =>
        Opportunity.Create(
            title: title,
            description: $"Description for {title}",
            location: "Dhaka",
            isRemote: false,
            startsAtUtc: startsAtUtc,
            id: id);

    private static GetOpportunitiesQueryHandler HandlerFor(params Opportunity[] stored)
    {
        var repository = new Mock<IOpportunityRepository>();
        repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        return new GetOpportunitiesQueryHandler(repository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSoonestOpportunityFirst()
    {
        var later = MakeOpportunity("later", "Later", new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc));
        var sooner = MakeOpportunity("sooner", "Sooner", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        var handler = HandlerFor(later, sooner);

        var result = await handler.Handle(new GetOpportunitiesQuery(), CancellationToken.None);

        Assert.Equal(["sooner", "later"], result.Select(o => o.Id));
    }

    [Fact]
    public async Task Handle_MapsEveryDomainFieldOntoTheDto()
    {
        var startsAt = new DateTime(2026, 6, 1, 9, 30, 0, DateTimeKind.Utc);
        var opportunity = Opportunity.Create(
            title: "Beach Cleanup",
            description: "Collect plastic waste along the shoreline.",
            location: "Cox's Bazar",
            isRemote: false,
            startsAtUtc: startsAt,
            id: "abc123");

        var result = await HandlerFor(opportunity).Handle(new GetOpportunitiesQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("abc123", dto.Id);
        Assert.Equal("Beach Cleanup", dto.Title);
        Assert.Equal("Collect plastic waste along the shoreline.", dto.Description);
        Assert.Equal("Cox's Bazar", dto.Location);
        Assert.False(dto.IsRemote);
        Assert.Equal(startsAt, dto.StartsAtUtc);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoOpportunitiesExist()
    {
        var result = await HandlerFor().Handle(new GetOpportunitiesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        var repository = new Mock<IOpportunityRepository>();
        repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        using var cts = new CancellationTokenSource();

        await new GetOpportunitiesQueryHandler(repository.Object)
            .Handle(new GetOpportunitiesQuery(), cts.Token);

        repository.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
    }
}
