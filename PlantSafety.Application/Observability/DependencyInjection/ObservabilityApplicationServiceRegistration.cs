// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Observability module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Observability.DependencyInjection;

/// <summary>
/// Extension methods to register Observability Application-layer services.
/// </summary>
public static class ObservabilityApplicationServiceRegistration
{
    /// <summary>
    /// Registers Observability-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddObservabilityApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}