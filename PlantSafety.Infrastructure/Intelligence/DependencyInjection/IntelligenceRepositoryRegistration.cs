// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DependencyInjection
//  Layer:          Infrastructure
//  File:           IntelligenceRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the 4 Intelligence read-model repositories
//                  (AITraining/AllInvestigations/DataLake/PredictiveSafety), newly implemented
//                  end-to-end (SQL tables + stored procedures + Dapper repositories) in this
//                  pass under the [Intelligence] SQL schema.
//                  Call <c>services.AddIntelligenceRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AITraining.Repositories;
using PlantSafety.Application.Intelligence.AllInvestigations.Repositories;
using PlantSafety.Application.Intelligence.DataLake.Repositories;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;
using PlantSafety.Infrastructure.Intelligence.AITraining.Persistence;
using PlantSafety.Infrastructure.Intelligence.AllInvestigations.Persistence;
using PlantSafety.Infrastructure.Intelligence.DataLake.Persistence;
using PlantSafety.Infrastructure.Intelligence.PredictiveSafety.Persistence;

namespace PlantSafety.Infrastructure.Intelligence.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 4 Intelligence read-model repositories.
/// </summary>
public static class IntelligenceRepositoryRegistration
{
    /// <summary>Registers all 4 Intelligence Dapper repositories into the DI container.</summary>
    public static IServiceCollection AddIntelligenceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAITrainingRepository, AITrainingDapperRepository>();
        services.AddScoped<IAllInvestigationsRepository, AllInvestigationsDapperRepository>();
        services.AddScoped<IDataLakeRepository, DataLakeDapperRepository>();
        services.AddScoped<IPredictiveSafetyRepository, PredictiveSafetyDapperRepository>();

        return services;
    }
}
