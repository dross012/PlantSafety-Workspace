// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.FMCSAStandards
//  Layer:          Infrastructure
//  File:           FMCSAStandardsInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.FMCSAStandards
//                  Infrastructure-layer repository implementations, mirroring
//                  OSHAStandardsInfrastructureServiceRegistration's precedent exactly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;
using PlantSafety.Infrastructure.Compliance.FMCSAStandards.Persistence;
using PlantSafety.Infrastructure.Compliance.FMCSAStandards.AuditGaps.Persistence;

namespace PlantSafety.Infrastructure.Compliance.FMCSAStandards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.FMCSAStandards submodule.</summary>
public static class FMCSAStandardsInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.FMCSAStandards Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddFMCSAStandardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFmcsaStandardsRepository, FmcsaStandardsDapperRepository>();
        services.AddScoped<IFmcsaAuditGapRepository, FmcsaAuditGapDapperRepository>();

        return services;
    }
}
