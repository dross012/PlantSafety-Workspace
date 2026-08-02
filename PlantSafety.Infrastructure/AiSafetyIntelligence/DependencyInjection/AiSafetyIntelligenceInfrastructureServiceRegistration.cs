// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligenceInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the AiSafetyIntelligence Infrastructure-
//                  layer repository implementations. Registers both same-simple-name
//                  IAiSafetyIntelligenceRepository interfaces (Domain write-side ->
//                  AiSafetyIntelligenceAggregateDapperRepository, Application read-side ->
//                  AiSafetyIntelligenceReadDapperRepository) using fully-qualified names to avoid
//                  an ambiguous-reference compile error. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.AiSafetyIntelligence.Persistence;

namespace PlantSafety.Infrastructure.AiSafetyIntelligence.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the AiSafetyIntelligence module.</summary>
public static class AiSafetyIntelligenceInfrastructureServiceRegistration
{
    /// <summary>Adds AiSafetyIntelligence Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddAiSafetyIntelligenceModuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.AiSafetyIntelligence.Repositories.IAiSafetyIntelligenceRepository, AiSafetyIntelligenceAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.AiSafetyIntelligence.Repositories.IAiSafetyIntelligenceRepository, AiSafetyIntelligenceReadDapperRepository>();

        return services;
    }
}
