// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceInfrastructureServiceRegistration.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Operational.PredictiveMaintenance
//                  Infrastructure-layer repository implementations. Registers all three
//                  same-simple-name-but-different-namespace IPredictiveMaintenanceAlertRepository
//                  interfaces (Domain write-side -> PredictiveMaintenanceAlertDapperRepository,
//                  Application read-side by-id -> PredictiveMaintenanceDapperRepository) plus the
//                  pre-existing Application-layer date-range list interface
//                  (IPredictiveMaintenanceRepository -> PredictiveMaintenanceDapperRepository,
//                  previously registered by OperationalLegacyRepositoryRegistration --
//                  re-registering it here is a harmless idempotent overwrite of the same
//                  concrete type, kept for discoverability alongside the new registrations) using
//                  fully-qualified names to avoid an ambiguous-reference compile error, exactly
//                  like PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired
//                  into the central InfrastructureServiceRegistration.cs directly (this pass wires
//                  it in immediately, not via a "human wires this in centrally" comment).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;

namespace PlantSafety.Infrastructure.Operational.PredictiveMaintenance.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Operational.PredictiveMaintenance module.</summary>
public static class PredictiveMaintenanceInfrastructureServiceRegistration
{
    /// <summary>Adds Operational.PredictiveMaintenance Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPredictiveMaintenanceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces.IPredictiveMaintenanceAlertRepository, PredictiveMaintenanceAlertDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.PredictiveMaintenance.Repositories.IPredictiveMaintenanceRepository, PredictiveMaintenanceDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.PredictiveMaintenance.Repositories.IPredictiveMaintenanceAlertRepository, PredictiveMaintenanceDapperRepository>();

        return services;
    }
}
