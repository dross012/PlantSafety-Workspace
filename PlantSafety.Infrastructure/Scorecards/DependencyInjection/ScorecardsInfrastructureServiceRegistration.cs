// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Infrastructure
//  File:           ScorecardsInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Scorecards Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Repositories;
using PlantSafety.Application.Scorecards.LaggingIndicators.Repositories;
using PlantSafety.Application.Scorecards.LeadingIndicators.Repositories;
using PlantSafety.Application.Scorecards.ReportingDistribution.Repositories;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Repositories;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Repositories;
using PlantSafety.Domain.Scorecards.Interfaces;
using PlantSafety.Infrastructure.Scorecards.Persistence;

namespace PlantSafety.Infrastructure.Scorecards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Scorecards module.</summary>
public static class ScorecardsInfrastructureServiceRegistration
{
    /// <summary>Adds Scorecards Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddScorecardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IScorecardRepository, ScorecardDapperRepository>();

        services.AddScoped<ScorecardsCategoryDapperRepository>();
        services.AddScoped<ILaggingIndicatorsRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());
        services.AddScoped<ILeadingIndicatorsRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());
        services.AddScoped<ICulturalPrecursorMetricsRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());
        services.AddScoped<IReportingDistributionRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());
        services.AddScoped<IScorecardDataSourcesRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());
        services.AddScoped<IScorecardStructureVisualizationRepository>(sp => sp.GetRequiredService<ScorecardsCategoryDapperRepository>());

        return services;
    }
}
