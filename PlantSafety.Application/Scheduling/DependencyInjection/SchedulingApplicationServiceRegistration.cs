// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Scheduling module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Scheduling.PredictiveScheduling.DependencyInjection;
using PlantSafety.Application.Scheduling.Rollup.DependencyInjection;

namespace PlantSafety.Application.Scheduling.DependencyInjection;

/// <summary>
/// Extension methods to register Scheduling Application-layer services.
/// </summary>
public static class SchedulingApplicationServiceRegistration
{
    /// <summary>
    /// Registers Scheduling-specific Application services, including the Phase 5 predictive
    /// scheduling engine (rule engine, prediction models, optimization strategies) and the
    /// Scheduling.Rollup composition layer over WorkOrders + PMScheduling (2026-08-03, see
    /// docs/Phase5/ui-gaps-scoping.md "Scheduling" section).
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSchedulingApplicationServices(this IServiceCollection services)
    {
        services.AddPredictiveSchedulingServices();
        services.AddSchedulingRollupApplicationServices();

        return services;
    }
}