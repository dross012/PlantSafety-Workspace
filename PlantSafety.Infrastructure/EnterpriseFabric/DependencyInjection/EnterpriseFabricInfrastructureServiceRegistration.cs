// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the EnterpriseFabric Infrastructure-layer
//                  repository implementations. Registers both same-simple-name
//                  IEnterpriseFabricRepository interfaces (Domain write-side ->
//                  EnterpriseFabricAggregateDapperRepository, Application read-side ->
//                  EnterpriseFabricReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, mirroring
//                  ComplianceAuditingInfrastructureServiceRegistration.cs's identical precedent.
//                  The extension method is deliberately named
//                  AddEnterpriseFabricAggregateInfrastructure (not AddEnterpriseFabricInfrastructure)
//                  to avoid any future collision with a differently-scoped registration for the
//                  pre-existing, unrelated Features.EnterpriseFabric.Users and
//                  Features.EnterpriseFabric.FabricNodes verticals.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.EnterpriseFabric.Persistence;

namespace PlantSafety.Infrastructure.EnterpriseFabric.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the EnterpriseFabric aggregate module.</summary>
public static class EnterpriseFabricInfrastructureServiceRegistration
{
    /// <summary>Adds EnterpriseFabric Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddEnterpriseFabricAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.EnterpriseFabric.Repositories.IEnterpriseFabricRepository, EnterpriseFabricAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnterpriseFabric.Repositories.IEnterpriseFabricRepository, EnterpriseFabricReadDapperRepository>();

        return services;
    }
}
