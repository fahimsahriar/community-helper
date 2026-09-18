using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Jobs;
using CommunityHelper.Infrastructure.Options;
using CommunityHelper.Infrastructure.Persistence;
using CommunityHelper.Infrastructure.Realtime;
using CommunityHelper.Infrastructure.Repositories;
using CommunityHelper.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IOpportunityRepository, InMemoryOpportunityRepository>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));

        var mongoConnectionString = configuration
            .GetSection(MongoDbSettings.SectionName)
            .GetValue<string>(nameof(MongoDbSettings.ConnectionString));

        if (string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            // Local dev and tests: hermetic in-memory stores.
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IVolunteerProfileRepository, InMemoryVolunteerProfileRepository>();
            services.AddSingleton<IOrganizationRepository, InMemoryOrganizationRepository>();
            services.AddSingleton<IRefreshTokenRepository, InMemoryRefreshTokenRepository>();
        }
        else
        {
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IUserRepository, MongoUserRepository>();
            services.AddSingleton<IVolunteerProfileRepository, MongoVolunteerProfileRepository>();
            services.AddSingleton<IOrganizationRepository, MongoOrganizationRepository>();
            services.AddSingleton<IRefreshTokenRepository, MongoRefreshTokenRepository>();
            services.AddHostedService<MongoIndexSetup>();
        }

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

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
