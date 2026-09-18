using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    string OrganizationId,
    string Name,
    string Type,
    List<string> CauseTags,
    string Location,
    string Description) : IRequest<OrganizationDto>, IOrganizationInput;
