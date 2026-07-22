// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials.DependencyInjection
//  Layer:          Infrastructure
//  File:           FinancialsRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the Financials.Financials read-model
//                  repository (distinct from Features.Financials.ROI, registered separately
//                  in FeaturesLegacyRepositoryRegistration), newly implemented end-to-end
//                  (SQL table + stored procedure + Dapper repository) in this pass under the
//                  [Financials] SQL schema.
//                  Call <c>services.AddFinancialsRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Financials.Financials.Repositories;
using PlantSafety.Infrastructure.Financials.Persistence;

namespace PlantSafety.Infrastructure.Financials.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the Financials.Financials read-model repository.
/// </summary>
public static class FinancialsRepositoryRegistration
{
    /// <summary>Registers the Financials Dapper repository into the DI container.</summary>
    public static IServiceCollection AddFinancialsRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFinancialsRepository, FinancialsDapperRepository>();

        return services;
    }
}
