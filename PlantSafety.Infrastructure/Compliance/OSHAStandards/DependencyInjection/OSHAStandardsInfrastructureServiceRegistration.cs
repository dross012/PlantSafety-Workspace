// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.OSHAStandards
//  Layer:          Infrastructure
//  File:           OSHAStandardsInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.OSHAStandards
//                  Infrastructure-layer repository implementation, mirroring
//                  ComplianceRulesEngineInfrastructureServiceRegistration's precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;
using PlantSafety.Infrastructure.Compliance.OSHAStandards.Persistence;
using PlantSafety.Infrastructure.Compliance.OSHAStandards.AuditGaps.Persistence;

namespace PlantSafety.Infrastructure.Compliance.OSHAStandards.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.OSHAStandards submodule.</summary>
public static class OSHAStandardsInfrastructureServiceRegistration
{
    /// <summary>Adds the Compliance.OSHAStandards Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddOSHAStandardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IOSHAStandardsRepository, OSHAStandardsDapperRepository>();
        services.AddScoped<IOshaAuditGapRepository, OshaAuditGapDapperRepository>();

        return services;
    }
}
