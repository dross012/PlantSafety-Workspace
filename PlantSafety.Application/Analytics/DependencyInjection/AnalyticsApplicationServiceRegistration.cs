// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers Analytics module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Analytics.DependencyInjection;

/// <summary>
/// Extension methods to register Analytics Application-layer services.
/// </summary>
public static class AnalyticsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Analytics-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddAnalyticsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
