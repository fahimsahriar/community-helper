using CommunityHelper.Application.Common.Interfaces;
using FluentValidation;

namespace CommunityHelper.Application.Features.Opportunities.Commands.SubmitOpportunitySearch;

public class SubmitOpportunitySearchCommandValidator
    : AbstractValidator<SubmitOpportunitySearchCommand>
{
    public SubmitOpportunitySearchCommandValidator(IClientConnectionRegistry connections)
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty()
            .WithMessage("A socket connection id is required.")
            .Must(connections.IsConnected)
            .WithMessage("Unknown or closed socket connection. Reconnect and try again.");

        RuleFor(x => x.Query)
            .MaximumLength(200);
    }
}
