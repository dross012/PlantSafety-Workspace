// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.NFPAStandards
//  Layer:          Infrastructure
//  File:           NFPAStandardsInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.NFPAStandards
//                  Infrastructure-layer repository implementations, mirroring
//                  OSHAStandardsInfrastructureServiceRegistration's precedent. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;
using PlantSafety.Infrastructure.Compliance.NFPAStandards.Persistence;
using PlantSafety.Infrastructure.Compliance.NFPAStandards.AuditGaps.Persistence;

namespace PlantSafety.Infrastructure.Compliance.NFPAStandards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.NFPAStandards submodule.</summary>
public static class NFPAStandardsInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.NFPAStandards Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddNFPAStandardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<INfpaReferenceRepository, NfpaReferenceDapperRepository>();
        services.AddScoped<INfpaAuditGapRepository, NfpaAuditGapDapperRepository>();

        return services;
    }
}
