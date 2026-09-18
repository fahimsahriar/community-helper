using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Queries.GetMyOrganization;

public sealed class GetMyOrganizationQueryHandler(
    ICurrentUserService currentUser,
    IOrganizationRepository organizations)
    : IRequestHandler<GetMyOrganizationQuery, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(
        GetMyOrganizationQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.OrgAdmin))
        {
            throw new UnauthorizedAccessException("Only organization admins can view their organization.");
        }

        var organization = await organizations.FindByOwnerUserIdAsync(currentUser.UserId, cancellationToken);
        if (organization is null)
        {
            throw new NotFoundException("Organization", currentUser.UserId);
        }

        return OrganizationRules.ToDto(organization);
    }
}
