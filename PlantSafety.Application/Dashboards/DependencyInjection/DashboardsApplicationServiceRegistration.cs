// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Dashboards module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Dashboards.DependencyInjection;

/// <summary>
/// Extension methods to register Dashboards Application-layer services.
/// </summary>
public static class DashboardsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Dashboards-specific Application services (Dashboard and Widget sub-components).
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddDashboardsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
