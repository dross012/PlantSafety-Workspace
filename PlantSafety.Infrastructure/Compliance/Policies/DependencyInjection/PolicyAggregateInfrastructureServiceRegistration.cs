// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Infrastructure
//  File:           PolicyAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Policy aggregate's
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IPolicyRepository interfaces (Domain write-side ->
//                  PolicyAggregateDapperRepository, Application read-side ->
//                  PolicyReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired into the
//                  central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Policies.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Policies.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Policy aggregate.</summary>
public static class PolicyAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Policy aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPolicyAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Policy.Interfaces.IPolicyRepository, PolicyAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Policy.Repositories.IPolicyRepository, PolicyReadDapperRepository>();

        return services;
    }
}
