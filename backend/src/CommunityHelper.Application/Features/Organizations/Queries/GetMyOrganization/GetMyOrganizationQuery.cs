using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Queries.GetMyOrganization;

public sealed record GetMyOrganizationQuery : IRequest<OrganizationDto>;
