// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses / SafetyEvents.RootCauseAnalysis
//  Layer:          Infrastructure
//  File:           SafetyEventsNearMissesRootCauseAnalysisInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  aggregate repository implementations for SafetyEvents.NearMisses and
//                  SafetyEvents.RootCauseAnalysis (NearMissesAggregateDapperRepository /
//                  RootCauseAnalysisAggregateDapperRepository implementing the Domain-layer
//                  INearMissesRepository / IRootCauseAnalysisRepository write-side interfaces)
//                  using fully-qualified names to avoid an ambiguous-reference compile error
//                  against the pre-existing, same-simple-name Application-layer read interfaces of
//                  the same name, exactly like PredictiveMaintenanceInfrastructureServiceRegistration.cs's
//                  identical precedent. The Application-layer read-side by-id method
//                  (GetByIdAsync) was added directly to the pre-existing
//                  INearMissesRepository/IRootCauseAnalysisRepository (Application) interfaces and
//                  is served by the pre-existing NearMissesDapperRepository/
//                  RootCauseAnalysisDapperRepository classes, already registered by
//                  SafetyEventsLegacyRepositoryRegistration.AddSafetyEventsLegacyRepositories() --
//                  no new registration is required for those. Wired into the central
//                  InfrastructureServiceRegistration.cs directly (this pass wires it in
//                  immediately, not via a "human wires this in centrally" comment).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.SafetyEvents.NearMisses.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence;

namespace PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;

/// <summary>
/// Registers Infrastructure-layer write-side aggregate repositories for the
/// SafetyEvents.NearMisses and SafetyEvents.RootCauseAnalysis modules.
/// </summary>
public static class SafetyEventsNearMissesRootCauseAnalysisInfrastructureServiceRegistration
{
    /// <summary>Adds the NearMisses/RootCauseAnalysis write-side Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddSafetyEventsNearMissesRootCauseAnalysisInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces.INearMissesRepository, NearMissesAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces.IRootCauseAnalysisRepository, RootCauseAnalysisAggregateDapperRepository>();

        return services;
    }
}
