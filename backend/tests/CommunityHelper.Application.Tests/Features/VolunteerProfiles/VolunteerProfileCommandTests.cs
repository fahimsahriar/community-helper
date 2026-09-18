using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Features.VolunteerProfiles.Commands.CreateVolunteerProfile;
using CommunityHelper.Application.Features.VolunteerProfiles.Commands.UpdateVolunteerProfile;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using Moq;

namespace CommunityHelper.Application.Tests.Features.VolunteerProfiles;

public class VolunteerProfileCommandTests
{
    private static CreateVolunteerProfileCommand ValidCreate() => new(
        ["Teaching"], "Weekends", ["Education"], "Dhaka", "Happy to help.");

    [Fact]
    public async Task Create_AsVolunteer_InsertsAndMapsDto()
    {
        var repo = new Mock<IVolunteerProfileRepository>();
        repo.Setup(r => r.FindByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((VolunteerProfile?)null);

        var result = await new CreateVolunteerProfileCommandHandler(
                new FakeCurrentUserService("user-1", UserRoles.Volunteer), repo.Object)
            .Handle(ValidCreate(), CancellationToken.None);

        Assert.Equal("user-1", result.UserId);
        Assert.Equal(["Teaching"], result.Skills);
        repo.Verify(r => r.InsertAsync(It.IsAny<VolunteerProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_AsOrgAdmin_ThrowsForbidden()
    {
        var repo = new Mock<IVolunteerProfileRepository>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new CreateVolunteerProfileCommandHandler(
                    new FakeCurrentUserService("user-1", UserRoles.OrgAdmin), repo.Object)
                .Handle(ValidCreate(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_WithUnknownRole_ThrowsForbidden()
    {
        var repo = new Mock<IVolunteerProfileRepository>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new CreateVolunteerProfileCommandHandler(
                    new FakeCurrentUserService("user-1", "corp_admin"), repo.Object)
                .Handle(ValidCreate(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_WhenProfileExists_ThrowsValidationException()
    {
        var repo = new Mock<IVolunteerProfileRepository>();
        repo.Setup(r => r.FindByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VolunteerProfile.Create("user-1", ["T"], "W", ["E"], "D", "B", "p-1"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            new CreateVolunteerProfileCommandHandler(
                    new FakeCurrentUserService("user-1", UserRoles.Volunteer), repo.Object)
                .Handle(ValidCreate(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_MissingProfile_ThrowsNotFound()
    {
        var repo = new Mock<IVolunteerProfileRepository>();
        repo.Setup(r => r.FindByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((VolunteerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateVolunteerProfileCommandHandler(
                    new FakeCurrentUserService("user-1", UserRoles.Volunteer), repo.Object)
                .Handle(new UpdateVolunteerProfileCommand(["T"], "W", ["E"], "D", "B"), CancellationToken.None));
    }

    [Fact]
    public async Task Update_Anonymous_ThrowsAuthenticationException()
    {
        var repo = new Mock<IVolunteerProfileRepository>();

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            new UpdateVolunteerProfileCommandHandler(
                    new FakeCurrentUserService(null, null), repo.Object)
                .Handle(new UpdateVolunteerProfileCommand(["T"], "W", ["E"], "D", "B"), CancellationToken.None));
    }
}
