// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence (AITraining / DataLake / PredictiveSafety)
//  Layer:          Infrastructure
//  File:           IntelligenceAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  aggregate repositories and by-id read repositories for the 3 previously-empty
//                  Intelligence stub modules: AITraining (AITrainingJob), DataLake
//                  (DataLakeActivity), PredictiveSafety (PredictiveSafetyAlert). Each write-side
//                  interface is registered with its fully-qualified Domain-layer name to avoid an
//                  ambiguous-reference compile error against the pre-existing, same-simple-name
//                  Application-layer read interfaces (IAITrainingRepository/IDataLakeRepository/
//                  IPredictiveSafetyRepository, already registered by
//                  IntelligenceRepositoryRegistration.AddIntelligenceRepositories() -- untouched by
//                  this pass), exactly like
//                  ContinuousImprovementRCAAggregateInfrastructureServiceRegistration.cs's
//                  identical precedent for the sibling ContinuousImprovement.RCA module. Wired into
//                  the central InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AITraining.Repositories;
using PlantSafety.Application.Intelligence.DataLake.Repositories;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;
using PlantSafety.Infrastructure.Intelligence.AITraining.Persistence;
using PlantSafety.Infrastructure.Intelligence.DataLake.Persistence;
using PlantSafety.Infrastructure.Intelligence.PredictiveSafety.Persistence;

namespace PlantSafety.Infrastructure.Intelligence.DependencyInjection;

/// <summary>
/// Registers the Infrastructure-layer write-side aggregate repositories and by-id read
/// repositories for the AITraining, DataLake, and PredictiveSafety Intelligence sub-modules.
/// </summary>
public static class IntelligenceAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds the AITrainingJob/DataLakeActivity/PredictiveSafetyAlert write-side aggregate repositories and by-id read repositories to the service collection.</summary>
    public static IServiceCollection AddIntelligenceAggregatesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Intelligence.AITraining.Interfaces.IAITrainingJobRepository, AITrainingJobAggregateDapperRepository>();
        services.AddScoped<IAITrainingJobDetailRepository, AITrainingJobDetailDapperRepository>();

        services.AddScoped<PlantSafety.Domain.Intelligence.DataLake.Interfaces.IDataLakeActivityRepository, DataLakeActivityAggregateDapperRepository>();
        services.AddScoped<IDataLakeActivityDetailRepository, DataLakeActivityDetailDapperRepository>();

        services.AddScoped<PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces.IPredictiveSafetyAlertRepository, PredictiveSafetyAlertAggregateDapperRepository>();
        services.AddScoped<IPredictiveSafetyAlertDetailRepository, PredictiveSafetyAlertDetailDapperRepository>();

        return services;
    }
}
