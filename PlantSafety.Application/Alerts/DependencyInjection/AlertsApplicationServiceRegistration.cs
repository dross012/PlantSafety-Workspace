// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Alerts module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Alerts.DependencyInjection;

/// <summary>
/// Extension methods to register Alerts Application-layer services.
/// </summary>
public static class AlertsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Alerts-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddAlertsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
