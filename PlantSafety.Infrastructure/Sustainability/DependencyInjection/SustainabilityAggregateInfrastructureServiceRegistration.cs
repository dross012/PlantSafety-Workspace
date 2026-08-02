// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Sustainability Infrastructure-layer
//                  repository implementations for the Sustainability aggregate. Registers both
//                  same-simple-name ISustainabilityRepository interfaces (Domain write-side ->
//                  SustainabilityAggregateDapperRepository, Application read-side ->
//                  SustainabilityReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error. The extension method is deliberately named
//                  AddSustainabilityAggregateInfrastructure (not AddSustainabilityInfrastructure)
//                  to avoid colliding with the pre-existing Features-vertical
//                  AddFeaturesLegacyRepositories() registration for this module. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.DependencyInjection.ComplianceAuditingInfrastructureServiceRegistration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Sustainability.Persistence;

namespace PlantSafety.Infrastructure.Sustainability.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Sustainability (Sustainability aggregate) module.</summary>
public static class SustainabilityAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds Sustainability aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddSustainabilityAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Sustainability.Repositories.ISustainabilityRepository, SustainabilityAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Sustainability.Repositories.ISustainabilityRepository, SustainabilityReadDapperRepository>();

        return services;
    }
}
