// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Infrastructure
//  File:           ScoreCardsInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the top-level Analytics.ScoreCards
//                  Infrastructure-layer repository implementations (the ScoreCards aggregate
//                  root write path, distinct from the pre-existing read-only category sub-slices
//                  under Analytics.ScoreCards.{Compliance,Department,Supervisor,Training}, which
//                  are untouched by this pass). Registers both same-simple-name IScoreCardsRepository
//                  interfaces (Domain write-side -> ScoreCardsAggregateDapperRepository,
//                  Application read-side -> ScoreCardsAggregateReadDapperRepository) using
//                  fully-qualified names to avoid an ambiguous-reference compile error, exactly
//                  like PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired
//                  into the central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Analytics.ScoreCards.Persistence;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the top-level Analytics.ScoreCards aggregate.</summary>
public static class ScoreCardsInfrastructureServiceRegistration
{
    /// <summary>Adds Analytics.ScoreCards aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddScoreCardsAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Analytics.ScoreCards.Interfaces.IScoreCardsRepository, ScoreCardsAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Analytics.ScoreCards.Repositories.IScoreCardsRepository, ScoreCardsAggregateReadDapperRepository>();

        return services;
    }
}
