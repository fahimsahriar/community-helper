using CommunityHelper.Application.Features.VolunteerProfiles;
using CommunityHelper.Application.Features.VolunteerProfiles.Commands.CreateVolunteerProfile;
using CommunityHelper.Application.Features.VolunteerProfiles.Commands.UpdateVolunteerProfile;
using CommunityHelper.Application.Features.VolunteerProfiles.Queries.GetMyVolunteerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityHelper.API.Controllers;

[ApiController]
[Route("api/volunteer-profiles")]
[Authorize]
public class VolunteerProfilesController(ISender mediator) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(VolunteerProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VolunteerProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyVolunteerProfileQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("me")]
    [ProducesResponseType(typeof(VolunteerProfileDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<VolunteerProfileDto>> CreateMine(
        [FromBody] CreateVolunteerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMine), result);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(VolunteerProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VolunteerProfileDto>> UpdateMine(
        [FromBody] UpdateVolunteerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
