// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards
//  Layer:          Infrastructure
//  File:           StandardsReferenceInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Standards
//                  reference-data Infrastructure-layer repository implementation, mirroring
//                  EPAReferenceInfrastructureServiceRegistration / OSHAStandardsInfrastructure
//                  ServiceRegistration's precedent. NOTE: this extension method is not yet
//                  wired into the top-level InfrastructureServiceRegistration.cs composition
//                  root -- that wiring is pending a coordinated follow-up pass. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Standards.Repositories;
using PlantSafety.Infrastructure.Compliance.Standards.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Standards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Standards reference-data submodule.</summary>
public static class StandardsReferenceInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.Standards reference-data Dapper repository to the service collection.</summary>
    public static IServiceCollection AddStandardsReferenceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IStandardsReferenceRepository, StandardsReferenceDapperRepository>();

        return services;
    }
}
