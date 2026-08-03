// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RoutingAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the ProcessManagement.Routing
//                  Infrastructure-layer repository implementations for the RouteDefinition and
//                  RouteVersionSnapshot aggregates. Mirrors
//                  PlantSafety.Infrastructure.ProcessManagement.DependencyInjection.ProcessManagementAggregateInfrastructureServiceRegistration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the ProcessManagement.Routing submodule.</summary>
public static class RoutingAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds RouteDefinition/RouteVersionSnapshot aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddRoutingAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.ProcessManagement.Routing.Repositories.IRouteDefinitionRepository, RouteDefinitionAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Domain.ProcessManagement.Routing.Repositories.IRouteVersionSnapshotRepository, RouteVersionSnapshotAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.ProcessManagement.Routing.Repositories.IRouteDefinitionReadRepository, RouteDefinitionReadDapperRepository>();
        services.AddScoped<PlantSafety.Application.ProcessManagement.Routing.Repositories.IRouteVersionSnapshotReadRepository, RouteVersionSnapshotReadDapperRepository>();

        return services;
    }
}
