using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(string OrganizationId) : IRequest<OrganizationDto>;
