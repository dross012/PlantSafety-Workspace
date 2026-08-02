// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Analytics.Engagement aggregate root's
//                  Infrastructure-layer repository implementations. Registers both same-simple-name
//                  IEngagementRepository interfaces (Domain write-side ->
//                  EngagementAggregateDapperRepository, Application read-side ->
//                  EngagementReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs / ScoreCardsInfrastructureServiceRegistration.cs's
//                  identical precedent. Distinct from the pre-existing, unrelated legacy
//                  Application.Analytics.Repositories.IEngagementRepository /
//                  Infrastructure.Analytics.Persistence.EngagementDapperRepository pair registered
//                  by AnalyticsLegacyRepositoryRegistration.AddAnalyticsLegacyRepositories() (a
//                  different, generic engagement-metric read model). Wired into the central
//                  InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Analytics.Engagement.Persistence;

namespace PlantSafety.Infrastructure.Analytics.Engagement.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Analytics.Engagement aggregate.</summary>
public static class EngagementInfrastructureServiceRegistration
{
    /// <summary>Adds Analytics.Engagement aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddEngagementAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Analytics.Engagement.Interfaces.IEngagementRepository, EngagementAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Analytics.Engagement.Repositories.IEngagementRepository, EngagementReadDapperRepository>();

        return services;
    }
}
