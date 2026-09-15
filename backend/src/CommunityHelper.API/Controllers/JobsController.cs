using CommunityHelper.Application.Common.Models;
using CommunityHelper.Application.Features.Opportunities.Commands.SubmitOpportunitySearch;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunityHelper.API.Controllers;

/// <summary>
/// Submission half of the async request-reply pattern: these endpoints queue
/// work and return immediately. Results arrive on the caller's socket.
/// </summary>
[ApiController]
[Route("api/jobs")]
public class JobsController(ISender mediator) : ControllerBase
{
    /// <summary>Queues an opportunity search for the calling socket connection.</summary>
    [HttpPost("opportunity-search")]
    [ProducesResponseType(typeof(JobAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobAcceptedResponse>> SubmitOpportunitySearch(
        [FromBody] SubmitOpportunitySearchCommand command,
        CancellationToken cancellationToken)
        => Accepted(new JobAcceptedResponse(await mediator.Send(command, cancellationToken)));
}
