using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;

namespace CommunityHelper.Infrastructure.Repositories;

/// <summary>
/// Seeded in-memory store standing in for MongoDB until persistence lands
/// (see docs/plan.md, Phase 0 — Backend Bootstrap). Swapping in a
/// MongoOpportunityRepository is a one-line change in DependencyInjection.
/// </summary>
public class InMemoryOpportunityRepository : IOpportunityRepository
{
    private static readonly IReadOnlyList<Opportunity> Seed =
    [
        Opportunity.Create(
            title: "Weekend Food Bank Sorting",
            description: "Help sort and pack donated goods for distribution to local families.",
            location: "Dhaka, Bangladesh",
            isRemote: false,
            startsAtUtc: new DateTime(2026, 10, 3, 9, 0, 0, DateTimeKind.Utc),
            id: "1"),
        Opportunity.Create(
            title: "Website Accessibility Audit",
            description: "Review a small charity site against WCAG 2.1 AA and write up findings.",
            location: "Remote",
            isRemote: true,
            startsAtUtc: new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc),
            id: "2"),
        Opportunity.Create(
            title: "After-School Maths Tutoring",
            description: "Tutor secondary students in algebra two afternoons a week.",
            location: "Chattogram, Bangladesh",
            isRemote: false,
            startsAtUtc: new DateTime(2026, 9, 28, 15, 30, 0, DateTimeKind.Utc),
            id: "3"),
    ];

    /// <summary>
    /// Stands in for the round trip a real store (and the PRD's matching engine)
    /// would cost. Keeps the async job round trip observable while the data is
    /// held in memory; it disappears with this class.
    /// </summary>
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(1200);

    public Task<IReadOnlyList<Opportunity>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Seed);
    }

    public async Task<IReadOnlyList<Opportunity>> SearchAsync(string query, CancellationToken ct = default)
    {
        await Task.Delay(SimulatedLatency, ct);

        if (string.IsNullOrWhiteSpace(query))
        {
            return Seed;
        }

        return Seed
            .Where(o =>
                o.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || o.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                || o.Location.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
