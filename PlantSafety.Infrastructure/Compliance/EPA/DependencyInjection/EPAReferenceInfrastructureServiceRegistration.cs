// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA
//  Layer:          Infrastructure
//  File:           EPAReferenceInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.EPA reference-data
//                  Infrastructure-layer repository implementation, mirroring
//                  OSHAStandardsInfrastructureServiceRegistration's precedent. NOTE: this
//                  extension method is not yet wired into the top-level
//                  InfrastructureServiceRegistration.cs composition root -- that wiring is
//                  pending a coordinated follow-up pass (a parallel effort is also adding EPA
//                  registrations there). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.EPA.Repositories;
using PlantSafety.Infrastructure.Compliance.EPA.Persistence;

namespace PlantSafety.Infrastructure.Compliance.EPA.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.EPA reference-data submodule.</summary>
public static class EPAReferenceInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.EPA reference-data Dapper repository to the service collection.</summary>
    public static IServiceCollection AddEpaReferenceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEpaReferenceRepository, EpaReferenceDapperRepository>();

        return services;
    }
}
