// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Infrastructure
//  File:           PredictiveAnalyticsInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Analytics.PredictiveAnalytics
//                  aggregate root's Infrastructure-layer repository implementations. The Domain
//                  write-side IPredictiveAnalyticsRepository resolves to the new
//                  PredictiveAnalyticsAggregateDapperRepository. The Application read-side
//                  IPredictiveAnalyticsRepository (GetPredictionsAsync + the new GetByIdAsync)
//                  already resolves to PredictiveAnalyticsDapperRepository via
//                  AnalyticsLegacyRepositoryRegistration.AddAnalyticsLegacyRepositories() -- not
//                  re-registered here to avoid a duplicate-registration conflict; this extension
//                  only adds the previously-missing write-side registration. Mirrors
//                  PermitsInfrastructureServiceRegistration.cs / ScoreCardsInfrastructureServiceRegistration.cs's
//                  precedent. Wired into the central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;

namespace PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Analytics.PredictiveAnalytics aggregate.</summary>
public static class PredictiveAnalyticsInfrastructureServiceRegistration
{
    /// <summary>Adds the Analytics.PredictiveAnalytics aggregate's write-side Dapper repository to the service collection.</summary>
    public static IServiceCollection AddPredictiveAnalyticsAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces.IPredictiveAnalyticsRepository, PredictiveAnalyticsAggregateDapperRepository>();

        return services;
    }
}
