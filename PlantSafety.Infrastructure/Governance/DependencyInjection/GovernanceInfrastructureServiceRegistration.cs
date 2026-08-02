// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Infrastructure
//  File:           GovernanceInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Governance Infrastructure-layer
//                  repository implementations. Registers both same-simple-name IGovernanceRepository
//                  interfaces (Domain write-side -> GovernanceAggregateDapperRepository,
//                  Application read-side -> GovernanceReadDapperRepository) using
//                  fully-qualified names to avoid an ambiguous-reference compile error, exactly
//                  like ComplianceAuditingInfrastructureServiceRegistration.cs's identical
//                  precedent. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Governance.Persistence;

namespace PlantSafety.Infrastructure.Governance.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Governance module.</summary>
public static class GovernanceInfrastructureServiceRegistration
{
    /// <summary>Adds Governance Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddGovernanceAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Governance.Repositories.IGovernanceRepository, GovernanceAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Governance.Repositories.IGovernanceRepository, GovernanceReadDapperRepository>();

        return services;
    }
}
