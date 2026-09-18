using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Features.Organizations.Commands.RegisterOrganization;
using CommunityHelper.Application.Features.Organizations.Commands.UpdateOrganization;
using CommunityHelper.Application.Tests.Fakes;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using Moq;

namespace CommunityHelper.Application.Tests.Features.Organizations;

public class OrganizationCommandTests
{
    private static RegisterOrganizationCommand ValidRegister() => new(
        "Food Bank", "Nonprofit", ["Hunger"], "Dhaka", "Feeding the city.");

    [Fact]
    public async Task Register_AsOrgAdmin_CreatesPendingOrganization()
    {
        var repo = new Mock<IOrganizationRepository>();
        repo.Setup(r => r.FindByOwnerUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var result = await new RegisterOrganizationCommandHandler(
                new FakeCurrentUserService("user-1", UserRoles.OrgAdmin), repo.Object)
            .Handle(ValidRegister(), CancellationToken.None);

        Assert.Equal("Food Bank", result.Name);
        Assert.Null(result.VerifiedAtUtc);
        Assert.False(result.IsVerified);
        repo.Verify(r => r.InsertAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_AsVolunteer_ThrowsForbidden()
    {
        var repo = new Mock<IOrganizationRepository>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new RegisterOrganizationCommandHandler(
                    new FakeCurrentUserService("user-1", UserRoles.Volunteer), repo.Object)
                .Handle(ValidRegister(), CancellationToken.None));
    }

    [Fact]
    public async Task Register_WithUnknownRole_ThrowsForbidden()
    {
        var repo = new Mock<IOrganizationRepository>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new RegisterOrganizationCommandHandler(
                    new FakeCurrentUserService("user-1", "corp_admin"), repo.Object)
                .Handle(ValidRegister(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_ByNonOwner_ThrowsForbidden()
    {
        var repo = new Mock<IOrganizationRepository>();
        repo.Setup(r => r.FindByIdAsync("org-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Organization.Create("owner-1", "Food Bank", "Nonprofit", ["Hunger"], "Dhaka", "Desc", "org-1"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateOrganizationCommandHandler(
                    new FakeCurrentUserService("user-2", UserRoles.OrgAdmin), repo.Object)
                .Handle(new UpdateOrganizationCommand("org-1", "N", "T", ["C"], "L", "D"), CancellationToken.None));
    }

    [Fact]
    public async Task Update_MissingOrganization_ThrowsNotFound()
    {
        var repo = new Mock<IOrganizationRepository>();
        repo.Setup(r => r.FindByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateOrganizationCommandHandler(
                    new FakeCurrentUserService("user-1", UserRoles.OrgAdmin), repo.Object)
                .Handle(new UpdateOrganizationCommand("missing", "N", "T", ["C"], "L", "D"), CancellationToken.None));
    }
}
