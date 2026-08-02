// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Infrastructure
//  File:           PermitsInfrastructureServiceRegistration.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Permits Infrastructure-layer
//                  repository implementations. Registers both same-simple-name IPermitsRepository
//                  interfaces (Domain write-side -> PermitsDapperRepository, Application read-side
//                  -> PermitsReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  RiskManagementInfrastructureServiceRegistration.cs /
//                  AccidentManagementInfrastructureServiceRegistration.cs's identical precedent.
//                  Wired into the central InfrastructureServiceRegistration.cs (this pass, unlike
//                  the "a human wires this in centrally" comment on the older per-module
//                  extensions, wires it directly since this is a from-scratch build-out).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Permits.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Permits.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Permits module.</summary>
public static class PermitsInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Permits Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPermitsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Permits.Interfaces.IPermitsRepository, PermitsDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository, PermitsReadDapperRepository>();

        return services;
    }
}
