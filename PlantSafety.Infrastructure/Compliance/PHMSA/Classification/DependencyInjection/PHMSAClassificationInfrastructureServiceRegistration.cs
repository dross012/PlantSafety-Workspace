// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Infrastructure
//  File:           PHMSAClassificationInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.PHMSA.Classification
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IHazmatMaterialRepository interfaces (Domain write-side ->
//                  HazmatMaterialDapperRepository, Application read-side ->
//                  HazmatMaterialReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.PHMSA.Classification.Persistence;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Classification.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.PHMSA.Classification module.</summary>
public static class PHMSAClassificationInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.PHMSA.Classification Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPHMSAClassificationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces.IHazmatMaterialRepository, HazmatMaterialDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.PHMSA.Classification.Repositories.IHazmatMaterialRepository, HazmatMaterialReadDapperRepository>();

        return services;
    }
}
