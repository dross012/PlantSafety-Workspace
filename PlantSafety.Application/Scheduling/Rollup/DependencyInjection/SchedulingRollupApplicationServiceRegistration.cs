// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Registers Scheduling.Rollup module Application-layer services. Self-contained
//   extension method; wired into the composition root by the host application. Mirrors
//   SafetyProgramsApplicationServiceRegistration -- MediatR handlers are auto-registered via
//   assembly scan and every dependency (ISender, IWorkOrderReadRepository via WorkOrder queries,
//   IPredictiveMaintenanceAlertRepository) is already registered by its owning module, so this
//   method is currently a no-op placeholder kept for structural symmetry with sibling modules.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Scheduling.Rollup.DependencyInjection;

/// <summary>
/// Extension methods to register Scheduling.Rollup Application-layer services.
/// </summary>
public static class SchedulingRollupApplicationServiceRegistration
{
    /// <summary>
    /// Registers Scheduling.Rollup-specific Application services.
    /// MediatR handlers are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSchedulingRollupApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
