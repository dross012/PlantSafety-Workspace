// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           ManagementOfChangeInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.ManagementOfChange
//                  Infrastructure-layer repository implementations. Registers both same-simple-name
//                  IManagementOfChangeRepository interfaces (Domain write-side ->
//                  ManagementOfChangeDapperRepository, Application read-side ->
//                  ManagementOfChangeReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired into the
//                  central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.ManagementOfChange.Persistence;

namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.ManagementOfChange module.</summary>
public static class ManagementOfChangeInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.ManagementOfChange Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddManagementOfChangeAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces.IManagementOfChangeRepository, ManagementOfChangeDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.ManagementOfChange.Repositories.IManagementOfChangeRepository, ManagementOfChangeReadDapperRepository>();

        return services;
    }
}
