// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the AiGovernance Infrastructure-layer
//                  repository implementations. Registers both same-simple-name IAiGovernanceRepository
//                  interfaces (Domain write-side -> AiGovernanceAggregateDapperRepository,
//                  Application read-side -> AiGovernanceReadDapperRepository) using fully-qualified
//                  names to avoid an ambiguous-reference compile error, exactly like
//                  ComplianceAuditingInfrastructureServiceRegistration.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.AiGovernance.Persistence;

namespace PlantSafety.Infrastructure.AiGovernance.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the AiGovernance module.</summary>
public static class AiGovernanceInfrastructureServiceRegistration
{
    /// <summary>Adds AiGovernance Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddAiGovernanceModuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.AiGovernance.Repositories.IAiGovernanceRepository, AiGovernanceAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.AiGovernance.Repositories.IAiGovernanceRepository, AiGovernanceReadDapperRepository>();

        return services;
    }
}
