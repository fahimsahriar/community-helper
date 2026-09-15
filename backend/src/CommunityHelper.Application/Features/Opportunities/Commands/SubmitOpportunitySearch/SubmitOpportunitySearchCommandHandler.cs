using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Application.Common.Models;
using CommunityHelper.Application.Features.Opportunities.Queries.SearchOpportunities;
using MediatR;

namespace CommunityHelper.Application.Features.Opportunities.Commands.SubmitOpportunitySearch;

public class SubmitOpportunitySearchCommandHandler(IBackgroundJobQueue queue)
    : IRequestHandler<SubmitOpportunitySearchCommand, string>
{
    public async Task<string> Handle(
        SubmitOpportunitySearchCommand request,
        CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid().ToString("N");

        await queue.EnqueueAsync(
            new JobWorkItem(jobId, request.ConnectionId, new SearchOpportunitiesQuery(request.Query)),
            cancellationToken);

        return jobId;
    }
}
