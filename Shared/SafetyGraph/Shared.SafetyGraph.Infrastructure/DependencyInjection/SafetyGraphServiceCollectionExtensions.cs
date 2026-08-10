// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         DependencyInjection
//  File:           SafetyGraphServiceCollectionExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Marker registration extension,
//                  mirroring SchedulingServiceCollectionExtensions/
//                  PredictiveSafetyServiceCollectionExtensions (Phases
//                  14/15) exactly. Shared.SafetyGraph exposes only pure
//                  static classes -- nothing stateful to register today.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

namespace Shared.SafetyGraph.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.SafetyGraph services into the DI container.</summary>
public static class SafetyGraphServiceCollectionExtensions
{
    /// <summary>
    /// Registers Shared.SafetyGraph into <paramref name="services"/>. Currently a no-op beyond
    /// returning <paramref name="services"/> unchanged -- every graph engine, orchestrator, and
    /// decorator is a pure static class with no stateful dependency to register yet.
    /// </summary>
    public static IServiceCollection AddSafetyGraph(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
