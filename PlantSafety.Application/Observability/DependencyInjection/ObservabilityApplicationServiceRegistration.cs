// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Observability module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Observability.Telemetry;

namespace PlantSafety.Application.Observability.DependencyInjection;

/// <summary>
/// Extension methods to register Observability Application-layer services.
/// </summary>
public static class ObservabilityApplicationServiceRegistration
{
    /// <summary>
    /// Registers Observability-specific Application services, including the Phase 5 production
    /// telemetry stream processor (Section 2.2: real-time anomaly detection).
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddObservabilityApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ProductionTelemetryStreamProcessor>();

        return services;
    }
}