using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdQueryValidator : AbstractValidator<GetOrganizationByIdQuery>
{
    public GetOrganizationByIdQueryValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}

public sealed class GetOrganizationByIdQueryHandler(IOrganizationRepository organizations)
    : IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(
        GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.FindByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            throw new NotFoundException("Organization", request.OrganizationId);
        }

        return OrganizationRules.ToDto(organization);
    }
}
