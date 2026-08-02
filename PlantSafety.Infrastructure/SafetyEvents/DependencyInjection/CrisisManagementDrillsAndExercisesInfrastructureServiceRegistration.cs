// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement / SafetyEvents.DrillsAndExercises
//  Layer:          Infrastructure
//  File:           CrisisManagementDrillsAndExercisesInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  (Domain) aggregate repositories for CrisisManagement and DrillsAndExercises --
//                  Domain.SafetyEvents.CrisisManagement.Interfaces.ICrisisManagementRepository ->
//                  CrisisManagementAggregateDapperRepository and
//                  Domain.SafetyEvents.DrillsAndExercises.Interfaces.IDrillsAndExercisesRepository
//                  -> DrillsAndExercisesAggregateDapperRepository, using fully-qualified names to
//                  avoid an ambiguous-reference compile error against the pre-existing
//                  Application-layer read-only ICrisisManagementRepository/
//                  IDrillsAndExercisesRepository interfaces (already registered by
//                  SafetyEventsLegacyRepositoryRegistration.AddSafetyEventsLegacyRepositories,
//                  backed by the same-named-but-different-class CrisisManagementDapperRepository/
//                  DrillsAndExercisesDapperRepository, which this pass additionally extended with
//                  GetByIdAsync -- no new registration needed for those, the existing
//                  registration now transparently serves the new by-id query too). Mirrors
//                  PredictiveMaintenanceInfrastructureServiceRegistration.cs's identical precedent.
//                  Wired into the central InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.SafetyEvents.CrisisManagement.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.DrillsAndExercises.Persistence;

namespace PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;

/// <summary>Registers Infrastructure-layer write-side services for the CrisisManagement and DrillsAndExercises modules.</summary>
public static class CrisisManagementDrillsAndExercisesInfrastructureServiceRegistration
{
    /// <summary>Adds the CrisisManagement/DrillsAndExercises write-side (Domain) Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddCrisisManagementDrillsAndExercisesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.SafetyEvents.CrisisManagement.Interfaces.ICrisisManagementRepository, CrisisManagementAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces.IDrillsAndExercisesRepository, DrillsAndExercisesAggregateDapperRepository>();

        return services;
    }
}
