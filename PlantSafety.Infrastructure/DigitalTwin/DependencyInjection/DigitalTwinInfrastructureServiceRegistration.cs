// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the DigitalTwin Infrastructure-layer
//                  repository implementations. Registers both same-simple-name IDigitalTwinRepository
//                  interfaces (Domain write-side -> DigitalTwinAggregateDapperRepository,
//                  Application read-side -> DigitalTwinReadDapperRepository) using fully-qualified
//                  names to avoid an ambiguous-reference compile error. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.DigitalTwin.Persistence;

namespace PlantSafety.Infrastructure.DigitalTwin.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the DigitalTwin module.</summary>
public static class DigitalTwinInfrastructureServiceRegistration
{
    /// <summary>Adds DigitalTwin Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddDigitalTwinModuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.DigitalTwin.Repositories.IDigitalTwinRepository, DigitalTwinAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.DigitalTwin.Repositories.IDigitalTwinRepository, DigitalTwinReadDapperRepository>();

        return services;
    }
}
