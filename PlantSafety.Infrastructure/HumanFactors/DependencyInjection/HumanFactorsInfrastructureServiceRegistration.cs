// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the HumanFactors Infrastructure-layer
//                  repository implementations. Registers both same-simple-name
//                  IHumanFactorsRepository interfaces (Domain write-side ->
//                  HumanFactorsAggregateDapperRepository, Application read-side ->
//                  HumanFactorsReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  ComplianceAuditingInfrastructureServiceRegistration.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.HumanFactors.Persistence;

namespace PlantSafety.Infrastructure.HumanFactors.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the HumanFactors module.</summary>
public static class HumanFactorsInfrastructureServiceRegistration
{
    /// <summary>Adds HumanFactors Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddHumanFactorsAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.HumanFactors.Repositories.IHumanFactorsRepository, HumanFactorsAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.HumanFactors.Repositories.IHumanFactorsRepository, HumanFactorsReadDapperRepository>();

        return services;
    }
}
