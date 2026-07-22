// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.DependencyInjection
//  Layer:          Infrastructure
//  File:           ContinuousImprovementRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the 3 ContinuousImprovement repositories
//                  (CAPA/RCA read-models + the ExportContinuousImprovement write-through
//                  export repository), newly implemented end-to-end (SQL tables + stored
//                  procedures + Dapper repositories) in this pass under the
//                  [ContinuousImprovement] SQL schema.
//                  Call <c>services.AddContinuousImprovementRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.CAPA.Repositories;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;
using PlantSafety.Application.ContinuousImprovement.Repositories;
using PlantSafety.Infrastructure.ContinuousImprovement.CAPA.Persistence;
using PlantSafety.Infrastructure.ContinuousImprovement.Persistence;
using PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence;

namespace PlantSafety.Infrastructure.ContinuousImprovement.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 3 ContinuousImprovement repositories.
/// </summary>
public static class ContinuousImprovementRepositoryRegistration
{
    /// <summary>Registers all 3 ContinuousImprovement Dapper repositories into the DI container.</summary>
    public static IServiceCollection AddContinuousImprovementRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICAPARepository, CAPADapperRepository>();
        services.AddScoped<IRCARepository, RCADapperRepository>();
        services.AddScoped<IExportContinuousImprovementRepository, ExportContinuousImprovementDapperRepository>();

        return services;
    }
}
