// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Infrastructure
//  File:           HumanFactorsCognitiveLoadAnalyticsInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the HumanFactors.CognitiveLoadAnalytics
//                  Infrastructure-layer repository implementations: the two Domain-layer
//                  write-side repositories (ICognitiveLoadLedgerRepository,
//                  ICognitiveRiskPredictionRepository) and the Application-layer read-side
//                  repository (ICognitiveLoadAnalyticsReadRepository). Also registers the
//                  CognitiveLoadAnalyticsService domain service singleton (pure, stateless,
//                  side-effect-free -- consumed by GenerateCognitiveRiskPredictionCommandHandler).
//                  Mirrors SustainabilityAdvancedMetricsInfrastructureServiceRegistration's
//                  naming/shape precedent for a sub-module sitting alongside a generic parent
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Services;
using PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.Persistence;

namespace PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the HumanFactors.CognitiveLoadAnalytics sub-module.</summary>
public static class HumanFactorsCognitiveLoadAnalyticsInfrastructureServiceRegistration
{
    /// <summary>Adds HumanFactors.CognitiveLoadAnalytics Dapper repositories and domain services to the service collection.</summary>
    public static IServiceCollection AddHumanFactorsCognitiveLoadAnalyticsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces.ICognitiveLoadLedgerRepository, CognitiveLoadLedgerDapperRepository>();
        services.AddScoped<PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces.ICognitiveRiskPredictionRepository, CognitiveRiskPredictionDapperRepository>();
        services.AddScoped<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories.ICognitiveLoadAnalyticsReadRepository, CognitiveLoadAnalyticsReadDapperRepository>();

        services.AddSingleton<CognitiveLoadAnalyticsService>();

        return services;
    }
}
