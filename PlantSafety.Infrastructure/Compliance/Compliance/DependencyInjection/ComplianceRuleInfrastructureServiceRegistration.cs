// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceRuleInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the (self-named) Compliance.Compliance
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IComplianceRepository interfaces (Domain write-side ->
//                  ComplianceRuleAggregateDapperRepository, Application read-side ->
//                  ComplianceRuleReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. The
//                  extension method is deliberately named AddComplianceRuleInfrastructure (not
//                  AddComplianceInfrastructure) to avoid any ambiguity risk against the many other
//                  "Compliance"-prefixed registration extensions already imported into the
//                  central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Compliance.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Compliance.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the (self-named) Compliance.Compliance module.</summary>
public static class ComplianceRuleInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Compliance Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddComplianceRuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Compliance.Interfaces.IComplianceRepository, ComplianceRuleAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Compliance.Repositories.IComplianceRepository, ComplianceRuleReadDapperRepository>();

        return services;
    }
}
