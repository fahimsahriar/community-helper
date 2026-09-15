using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Jobs;
using CommunityHelper.Infrastructure.Realtime;
using CommunityHelper.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOpportunityRepository, InMemoryOpportunityRepository>();

        // One registry instance backs both the transport-facing and the
        // Application-facing view of the live connections.
        services.AddSingleton<WebSocketConnectionRegistry>();
        services.AddSingleton<IWebSocketConnectionRegistry>(sp =>
            sp.GetRequiredService<WebSocketConnectionRegistry>());
        services.AddSingleton<IClientConnectionRegistry>(sp =>
            sp.GetRequiredService<WebSocketConnectionRegistry>());

        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<JobProcessorService>();

        return services;
    }
}
