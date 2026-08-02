// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the OperationalSafety
//                  aggregate-root Infrastructure-layer repository implementations. Registers
//                  both same-simple-name IOperationalSafetyRepository interfaces (Domain
//                  write-side -> OperationalSafetyAggregateDapperRepository, Application
//                  read-side -> OperationalSafetyReadDapperRepository) using fully-qualified
//                  names to avoid an ambiguous-reference compile error, exactly like
//                  ComplianceAuditingInfrastructureServiceRegistration.cs's identical precedent.
//                  Named distinctly (AddOperationalSafetyAggregateInfrastructure, not
//                  AddOperationalSafetyInfrastructure) to avoid any future collision with the
//                  pre-existing, unrelated, LIVE OperationalSafety.Alerts/Checklists/
//                  PPEManagement/RiskManagement/Schedules registrations already wired in
//                  InfrastructureServiceRegistration.cs. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.OperationalSafety.Persistence;

namespace PlantSafety.Infrastructure.OperationalSafety.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the OperationalSafety aggregate-root module.</summary>
public static class OperationalSafetyAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds OperationalSafety aggregate-root Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddOperationalSafetyAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.OperationalSafety.Repositories.IOperationalSafetyRepository, OperationalSafetyAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.OperationalSafety.Repositories.IOperationalSafetyRepository, OperationalSafetyReadDapperRepository>();

        return services;
    }
}
