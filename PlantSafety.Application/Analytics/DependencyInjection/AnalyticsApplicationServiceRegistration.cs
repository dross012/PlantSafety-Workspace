// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers Analytics module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Analytics.Dashboard.Kpi;

namespace PlantSafety.Application.Analytics.DependencyInjection;

/// <summary>
/// Extension methods to register Analytics Application-layer services.
/// </summary>
public static class AnalyticsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Analytics-specific Application services, including the Phase 5 production KPI
    /// engine (Section 2.3: OEE, MTBF, MTTR, Throughput, Yield, Scrap Rate).
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddAnalyticsApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ProductionKpiEngine>();

        return services;
    }
}
