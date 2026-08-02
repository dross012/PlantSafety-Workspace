// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Infrastructure
//  File:           AIInvestigationsAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  aggregate repository implementation for Intelligence.AIInvestigations
//                  (AIInvestigationAggregateDapperRepository implementing the Domain-layer
//                  IAIInvestigationRepository write-side interface). Wired into the central
//                  InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Intelligence.AIInvestigations.Persistence;

namespace PlantSafety.Infrastructure.Intelligence.DependencyInjection;

/// <summary>Registers the Infrastructure-layer write-side aggregate repository for the Intelligence.AIInvestigations module.</summary>
public static class AIInvestigationsAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds the AIInvestigation aggregate's write-side Dapper repository to the service collection.</summary>
    public static IServiceCollection AddAIInvestigationsAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces.IAIInvestigationRepository, AIInvestigationAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Intelligence.AIInvestigations.Repositories.IAIInvestigationReadRepository, PlantSafety.Infrastructure.Intelligence.AIInvestigations.Persistence.AIInvestigationReadDapperRepository>();

        return services;
    }
}
