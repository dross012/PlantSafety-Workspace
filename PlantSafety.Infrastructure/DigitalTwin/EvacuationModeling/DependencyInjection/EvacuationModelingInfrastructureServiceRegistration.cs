// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.DependencyInjection
//  Layer:          Infrastructure
//  File:           EvacuationModelingInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the DigitalTwin.EvacuationModeling
//                  Infrastructure layer (4 aggregate repositories, each implementing both its
//                  Domain write-repository interface and its Application read-model repository
//                  interface against the same Dapper repository class). Call
//                  <c>services.AddEvacuationModelingInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationRoutes.Persistence;
using PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationZones.Persistence;
using PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EgressSimulations.Persistence;
using PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Persistence;

namespace PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.DependencyInjection;

/// <summary>Per-module DI entry point for the DigitalTwin.EvacuationModeling Infrastructure layer.</summary>
public static class EvacuationModelingInfrastructureServiceRegistration
{
    /// <summary>Registers all DigitalTwin.EvacuationModeling Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddEvacuationModelingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces.IEvacuationRouteRepository, EvacuationRouteDapperRepository>();
        services.AddScoped<PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories.IEvacuationRouteRepository, EvacuationRouteDapperRepository>();

        services.AddScoped<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces.IEvacuationZoneRepository, EvacuationZoneDapperRepository>();
        services.AddScoped<PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Repositories.IEvacuationZoneRepository, EvacuationZoneDapperRepository>();

        services.AddScoped<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces.IEgressSimulationRunRepository, EgressSimulationRunDapperRepository>();
        services.AddScoped<PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Repositories.IEgressSimulationRepository, EgressSimulationRunDapperRepository>();

        services.AddScoped<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces.IEvacuationBottleneckRepository, EvacuationBottleneckDapperRepository>();
        services.AddScoped<PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Repositories.IEvacuationBottleneckRepository, EvacuationBottleneckDapperRepository>();

        services.AddScoped<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services.EgressSimulationBottleneckAnalysisService>();

        return services;
    }
}
