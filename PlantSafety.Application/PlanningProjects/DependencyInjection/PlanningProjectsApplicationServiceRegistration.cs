// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers PlanningProjects module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.PlanningProjects.DependencyInjection;

/// <summary>
/// Extension methods to register PlanningProjects Application-layer services.
/// </summary>
public static class PlanningProjectsApplicationServiceRegistration
{
    /// <summary>
    /// Registers PlanningProjects-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPlanningProjectsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
