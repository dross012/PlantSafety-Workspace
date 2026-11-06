// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         DependencyInjection
//  File:           SchedulingServiceCollectionExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Marker
//                  registration extension, mirroring every other module's
//                  Add<Module>() convention (e.g. FacilityDomainServiceCollectionExtensions.
//                  AddFacilityDomain()). Shared.Scheduling exposes only pure
//                  static classes (SchedulingRuleEngine, SchedulingOrchestrator,
//                  etc.) -- there is nothing stateful to register as a
//                  service today, so this method exists to give hosts
//                  (PlantSafety.UI.Api's Program.cs) one discoverable,
//                  conventional call site to wire Scheduling into the DI
//                  container once a stateful dependency (e.g. a persistence
//                  layer) is introduced in a later phase.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Scheduling.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.Scheduling services into the DI container.</summary>
public static class SchedulingServiceCollectionExtensions
{
    /// <summary>
    /// Registers Shared.Scheduling into <paramref name="services"/>. Currently a no-op beyond
    /// returning <paramref name="services"/> unchanged -- Scheduling's engines, orchestrator, and
    /// decorators are all pure static classes with no stateful dependency to register yet.
    /// </summary>
    public static IServiceCollection AddScheduling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
