using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler(
    ICurrentUserService currentUser,
    IOrganizationRepository organizations)
    : IRequestHandler<UpdateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.OrgAdmin))
        {
            throw new UnauthorizedAccessException("Only organization admins can edit an organization.");
        }

        var organization = await organizations.FindByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            throw new NotFoundException("Organization", request.OrganizationId);
        }

        if (!string.Equals(organization.OwnerUserId, currentUser.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Only the owning organization admin can edit this organization.");
        }

        organization.Update(
            request.Name,
            request.Type,
            request.CauseTags,
            request.Location,
            request.Description);

        await organizations.UpdateAsync(organization, cancellationToken);

        return OrganizationRules.ToDto(organization);
    }
}
