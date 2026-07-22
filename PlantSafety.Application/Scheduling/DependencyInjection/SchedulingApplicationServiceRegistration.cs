// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Scheduling module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Scheduling.DependencyInjection;

/// <summary>
/// Extension methods to register Scheduling Application-layer services.
/// </summary>
public static class SchedulingApplicationServiceRegistration
{
    /// <summary>
    /// Registers Scheduling-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSchedulingApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}