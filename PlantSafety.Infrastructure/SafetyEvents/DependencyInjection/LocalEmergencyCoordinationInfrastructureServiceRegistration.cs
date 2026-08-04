// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Infrastructure
//  File:           LocalEmergencyCoordinationInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering both the Domain write-side (aggregate) and
//                  Application read-model repositories for LocalEmergencyCoordination, using
//                  fully-qualified names to avoid an ambiguous-reference compile error between the
//                  two same-named-but-different ILocalEmergencyCoordinationRepository interfaces
//                  (Domain vs Application namespaces). Mirrors
//                  CrisisManagementDrillsAndExercisesInfrastructureServiceRegistration.cs /
//                  SafetyEventsWriteRepositoryRegistration.cs. Wired into the central
//                  InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.SafetyEvents.LocalEmergencyCoordination.Persistence;

namespace PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the LocalEmergencyCoordination module.</summary>
public static class LocalEmergencyCoordinationInfrastructureServiceRegistration
{
    /// <summary>Adds the LocalEmergencyCoordination write-side (Domain) and read-model (Application) Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddLocalEmergencyCoordinationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces.ILocalEmergencyCoordinationRepository, LocalEmergencyCoordinationWriteDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories.ILocalEmergencyCoordinationRepository, LocalEmergencyCoordinationDapperRepository>();

        return services;
    }
}
