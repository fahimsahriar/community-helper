using System.Reflection;
using CommunityHelper.Application.Common.Behaviours;
using CommunityHelper.Application.Features.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityHelper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<AuthTokenIssuer>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Order matters: logging wraps validation, which wraps exception logging.
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });

        return services;
    }
}
