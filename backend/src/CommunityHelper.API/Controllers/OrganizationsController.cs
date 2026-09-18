using CommunityHelper.Application.Features.Organizations;
using CommunityHelper.Application.Features.Organizations.Commands.RegisterOrganization;
using CommunityHelper.Application.Features.Organizations.Commands.UpdateOrganization;
using CommunityHelper.Application.Features.Organizations.Queries.GetMyOrganization;
using CommunityHelper.Application.Features.Organizations.Queries.GetOrganizationById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityHelper.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController(ISender mediator) : ControllerBase
{
    public sealed record UpdateOrganizationRequest(
        string Name,
        string Type,
        List<string> CauseTags,
        string Location,
        string Description);

    [HttpPost]
    [Authorize(Roles = "org_admin")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrganizationDto>> Register(
        [FromBody] RegisterOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("me")]
    [Authorize(Roles = "org_admin")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyOrganizationQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrganizationByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "org_admin")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationDto>> Update(
        string id,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateOrganizationCommand(
                id,
                request.Name,
                request.Type,
                request.CauseTags,
                request.Location,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
}
