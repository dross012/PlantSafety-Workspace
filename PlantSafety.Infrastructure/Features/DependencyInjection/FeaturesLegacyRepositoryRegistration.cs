// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.DependencyInjection
//  Layer:          Infrastructure
//  File:           FeaturesLegacyRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the 5 pre-existing Features-tree
//                  repository interfaces (AiGovernance/DigitalTwin/EnterpriseFabric.FabricNodes/
//                  Financials.ROI/Sustainability) that were scaffolded with Dapper
//                  implementations under PlantSafety.Infrastructure\Features\ but never wired
//                  into the DI container. Each repository's backing SQL schema is named after
//                  its feature ([AiGovernance]/[DigitalTwin]/[EnterpriseFabric]/[Financials]/
//                  [Sustainability]) rather than a shared "Features_*" prefix, matching the
//                  schema names already referenced by the corresponding stored-procedure calls
//                  in each Dapper repository class.
//                  Call <c>services.AddFeaturesLegacyRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.AiGovernance.Handlers;
using PlantSafety.Application.Features.DigitalTwin.Handlers;
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;
using PlantSafety.Application.Features.Financials.Handlers.ROI;
using PlantSafety.Application.Features.Sustainability.Handlers;
using PlantSafety.Infrastructure.Features.AiGovernance.Repositories;
using PlantSafety.Infrastructure.Features.DigitalTwin.Repositories;
using PlantSafety.Infrastructure.Features.EnterpriseFabric.FabricNodes.Repositories;
using PlantSafety.Infrastructure.Features.Financials.ROI.Repositories;
using PlantSafety.Infrastructure.Features.Sustainability.Repositories;

namespace PlantSafety.Infrastructure.Features.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 5 pre-existing Features-tree repository interfaces
/// that were scaffolded with Dapper implementations but never registered in the DI container.
/// </summary>
public static class FeaturesLegacyRepositoryRegistration
{
    /// <summary>Registers all 5 legacy Features-tree Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddFeaturesLegacyRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAiGovernanceRepository, AiGovernanceDapperRepository>();
        services.AddScoped<IDigitalTwinRepository, DigitalTwinDapperRepository>();
        services.AddScoped<IFabricNodeRepository, FabricNodeDapperRepository>();
        services.AddScoped<IRoiRepository, RoiDapperRepository>();
        services.AddScoped<ISustainabilityRepository, SustainabilityDapperRepository>();

        return services;
    }
}
