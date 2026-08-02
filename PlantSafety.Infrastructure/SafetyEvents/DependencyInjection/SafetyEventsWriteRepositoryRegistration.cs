// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement / SafetyEvents.Observation
//  Layer:          Infrastructure
//  File:           SafetyEventsWriteRepositoryRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Domain-layer write-side
//                  IEmergencyManagementRepository / IObservationRepository aggregate repository
//                  implementations added by the Phase 4 full build-out of these two SafetyEvents
//                  sub-aggregates. Distinct from SafetyEventsLegacyRepositoryRegistration.cs
//                  (which registers the pre-existing, unrelated Application-layer read-only
//                  IEmergencyManagementRepository / IObservationRepository interfaces backed by
//                  EmergencyManagementDapperRepository / ObservationItemsDapperRepository).
//                  Mirrors PermitsInfrastructureServiceRegistration.cs. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;

namespace PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;

/// <summary>Registers the Domain write-side EmergencyManagement/Observation Dapper repositories.</summary>
public static class SafetyEventsWriteRepositoryRegistration
{
    /// <summary>Adds the EmergencyManagement/Observation Domain write-side repositories to the service collection.</summary>
    public static IServiceCollection AddSafetyEventsWriteRepositories(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces.IEmergencyManagementRepository, EmergencyManagementWriteDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyEvents.Observation.Interfaces.IObservationRepository, ObservationWriteDapperRepository>();

        return services;
    }
}
