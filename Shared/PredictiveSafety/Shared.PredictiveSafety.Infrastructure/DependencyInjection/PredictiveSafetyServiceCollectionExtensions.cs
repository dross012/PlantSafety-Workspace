// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         DependencyInjection
//  File:           PredictiveSafetyServiceCollectionExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Marker registration extension,
//                  mirroring SchedulingServiceCollectionExtensions (Phase
//                  14) exactly. Shared.PredictiveSafety exposes only pure
//                  static classes -- nothing stateful to register today.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

namespace Shared.PredictiveSafety.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.PredictiveSafety services into the DI container.</summary>
public static class PredictiveSafetyServiceCollectionExtensions
{
    /// <summary>
    /// Registers Shared.PredictiveSafety into <paramref name="services"/>. Currently a no-op beyond
    /// returning <paramref name="services"/> unchanged -- every predictive engine, orchestrator, and
    /// decorator is a pure static class with no stateful dependency to register yet.
    /// </summary>
    public static IServiceCollection AddPredictiveSafety(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
