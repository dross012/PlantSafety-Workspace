// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the EnvironmentalHealth aggregate-root
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IEnvironmentalHealthRepository interfaces (Domain
//                  write-side -> EnvironmentalHealthAggregateDapperRepository, Application
//                  read-side -> EnvironmentalHealthReadDapperRepository) using fully-qualified
//                  names to avoid an ambiguous-reference compile error, exactly like
//                  ComplianceAuditingInfrastructureServiceRegistration.cs's identical precedent.
//                  Named distinctly (AddEnvironmentalHealthAggregateInfrastructure) to avoid any
//                  collision with the pre-existing, unrelated LIVE Features.EnvironmentalHealth
//                  "reading" vertical's DI registration (registered directly by fully-qualified
//                  name in InfrastructureServiceRegistration.cs, not via an extension method).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.EnvironmentalHealth.Persistence;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the EnvironmentalHealth aggregate-root module.</summary>
public static class EnvironmentalHealthAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds EnvironmentalHealth aggregate-root Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddEnvironmentalHealthAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.Repositories.IEnvironmentalHealthRepository, EnvironmentalHealthAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.Repositories.IEnvironmentalHealthRepository, EnvironmentalHealthReadDapperRepository>();

        return services;
    }
}
