// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Infrastructure
//  File:           PreventiveMaintenanceInfrastructureServiceRegistration.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Operational.PreventiveMaintenance
//                  Infrastructure-layer repository implementations added by the Phase 4
//                  PreventiveMaintenanceTask full build-out (write-side aggregate repository +
//                  by-id read repository). The pre-existing date-range list read repository
//                  (IPreventiveMaintenanceRepository -> PreventiveMaintenanceDapperRepository) is
//                  registered separately in OperationalLegacyRepositoryRegistration.cs and is left
//                  untouched by this extension. Registers both same-simple-name
//                  IPreventiveMaintenanceTaskRepository interfaces (Domain write-side ->
//                  PreventiveMaintenanceTaskDapperRepository, Application read-side ->
//                  PreventiveMaintenanceTaskReadDapperRepository) using fully-qualified names to
//                  avoid an ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired into
//                  the central InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence;

namespace PlantSafety.Infrastructure.Operational.PreventiveMaintenance.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Operational.PreventiveMaintenance module's PreventiveMaintenanceTask write path.</summary>
public static class PreventiveMaintenanceInfrastructureServiceRegistration
{
    /// <summary>Adds PreventiveMaintenanceTask Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPreventiveMaintenanceTaskInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces.IPreventiveMaintenanceTaskRepository, PreventiveMaintenanceTaskDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.PreventiveMaintenance.Repositories.IPreventiveMaintenanceTaskRepository, PreventiveMaintenanceTaskReadDapperRepository>();

        return services;
    }
}
