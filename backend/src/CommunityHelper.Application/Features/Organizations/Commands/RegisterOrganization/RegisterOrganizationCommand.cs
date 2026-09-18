using MediatR;

namespace CommunityHelper.Application.Features.Organizations.Commands.RegisterOrganization;

public sealed record RegisterOrganizationCommand(
    string Name,
    string Type,
    List<string> CauseTags,
    string Location,
    string Description) : IRequest<OrganizationDto>, IOrganizationInput;
