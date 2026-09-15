using CommunityHelper.Application.Features.Opportunities.Queries.GetOpportunities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunityHelper.API.Controllers;

[ApiController]
[Route("api/opportunities")]
public class OpportunitiesController(ISender mediator) : ControllerBase
{
    /// <summary>Returns every published volunteering opportunity.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OpportunityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OpportunityDto>>> GetOpportunities(
        CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetOpportunitiesQuery(), cancellationToken));
}
