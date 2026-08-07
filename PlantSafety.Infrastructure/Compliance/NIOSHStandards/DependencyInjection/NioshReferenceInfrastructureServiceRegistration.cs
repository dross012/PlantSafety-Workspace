// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.NIOSHStandards
//  Layer:          Infrastructure
//  File:           NioshReferenceInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.NIOSHStandards
//                  Infrastructure-layer repository implementation, mirroring
//                  OSHAStandardsInfrastructureServiceRegistration's precedent. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;
using PlantSafety.Infrastructure.Compliance.NIOSHStandards.Persistence;

namespace PlantSafety.Infrastructure.Compliance.NIOSHStandards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.NIOSHStandards submodule.</summary>
public static class NioshReferenceInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.NIOSHStandards Dapper repository to the service collection.</summary>
    public static IServiceCollection AddNioshReferenceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<INioshReferenceRepository, NioshReferenceDapperRepository>();

        return services;
    }
}
