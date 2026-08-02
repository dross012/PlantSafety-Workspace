// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Infrastructure
//  File:           ComplianceAuditingInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Auditing Infrastructure-layer
//                  repository implementations. Registers both same-simple-name IAuditingRepository
//                  interfaces (Domain write-side -> AuditingDapperRepository, Application read-side
//                  -> AuditingReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. The extension
//                  method is deliberately named AddComplianceAuditingInfrastructure (not
//                  AddAuditingInfrastructure) to avoid colliding with the pre-existing, unrelated
//                  top-level legacy-taxonomy AddAuditingInfrastructure() extension already imported
//                  into the central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Auditing.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Auditing.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Auditing module.</summary>
public static class ComplianceAuditingInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Auditing Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddComplianceAuditingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Auditing.Interfaces.IAuditingRepository, AuditingAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository, AuditingReadDapperRepository>();

        return services;
    }
}
