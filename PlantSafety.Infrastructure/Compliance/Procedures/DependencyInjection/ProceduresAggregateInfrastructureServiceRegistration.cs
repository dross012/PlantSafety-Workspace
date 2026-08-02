// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Procedures aggregate's
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IProceduresRepository interfaces (Domain write-side ->
//                  ProceduresAggregateDapperRepository, Application read-side ->
//                  ProceduresReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired into the
//                  central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Procedures.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Procedures.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Procedures aggregate.</summary>
public static class ProceduresAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Procedures aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddProceduresAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Procedures.Interfaces.IProceduresRepository, ProceduresAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Procedures.Repositories.IProceduresRepository, ProceduresReadDapperRepository>();

        return services;
    }
}
