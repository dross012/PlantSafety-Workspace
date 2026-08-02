// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Infrastructure
//  File:           ContinuousImprovementRCAAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  aggregate repository implementation for ContinuousImprovement.RCA
//                  (RCAAggregateDapperRepository implementing the Domain-layer IRCARepository
//                  write-side interface) using the fully-qualified interface name to avoid an
//                  ambiguous-reference compile error against the pre-existing, same-simple-name
//                  Application-layer read interface
//                  (PlantSafety.Application.ContinuousImprovement.RCA.Repositories.IRCARepository,
//                  already registered by ContinuousImprovementRepositoryRegistration
//                  .AddContinuousImprovementRepositories() -- untouched by this pass), exactly
//                  like SafetyEventsNearMissesRootCauseAnalysisInfrastructureServiceRegistration
//                  .cs's identical precedent for the sibling SafetyEvents.RootCauseAnalysis
//                  module. Wired into the central InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;
using PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence;

namespace PlantSafety.Infrastructure.ContinuousImprovement.DependencyInjection;

/// <summary>Registers the Infrastructure-layer write-side aggregate repository and by-id read repository for the ContinuousImprovement.RCA module.</summary>
public static class ContinuousImprovementRCAAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds the RCA write-side aggregate repository and by-id read repository to the service collection.</summary>
    public static IServiceCollection AddContinuousImprovementRCAAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces.IRCARepository, RCAAggregateDapperRepository>();
        services.AddScoped<IRCADetailRepository, RCADetailDapperRepository>();

        return services;
    }
}
