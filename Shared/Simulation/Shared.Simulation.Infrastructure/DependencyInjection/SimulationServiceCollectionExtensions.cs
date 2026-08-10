// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         DependencyInjection
//  File:           SimulationServiceCollectionExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Marker registration extension,
//                  mirroring SafetyGraphServiceCollectionExtensions (Phase
//                  16) exactly. Shared.Simulation exposes only pure static
//                  classes -- nothing stateful to register today.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Simulation.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.Simulation services into the DI container.</summary>
public static class SimulationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Shared.Simulation into <paramref name="services"/>. Currently a no-op beyond
    /// returning <paramref name="services"/> unchanged -- every simulation engine, orchestrator, and
    /// decorator is a pure static class with no stateful dependency to register yet.
    /// </summary>
    public static IServiceCollection AddSimulation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
