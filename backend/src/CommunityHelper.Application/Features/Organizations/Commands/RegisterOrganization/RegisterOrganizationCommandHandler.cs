using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Commands.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler(
    ICurrentUserService currentUser,
    IOrganizationRepository organizations)
    : IRequestHandler<RegisterOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(
        RegisterOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new AuthenticationException("Not authenticated.");
        }

        if (!currentUser.IsInRole(UserRoles.OrgAdmin))
        {
            throw new UnauthorizedAccessException("Only organization admins can register an organization.");
        }

        var existing = await organizations.FindByOwnerUserIdAsync(currentUser.UserId, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Organization", "An organization is already registered for this account."),
            });
        }

        var organization = Organization.Create(
            currentUser.UserId,
            request.Name,
            request.Type,
            request.CauseTags,
            request.Location,
            request.Description);

        await organizations.InsertAsync(organization, cancellationToken);

        return OrganizationRules.ToDto(organization);
    }
}
