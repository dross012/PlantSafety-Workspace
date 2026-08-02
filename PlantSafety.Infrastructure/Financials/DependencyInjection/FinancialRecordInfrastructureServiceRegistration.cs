// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialRecordInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the new FinancialRecord aggregate's
//                  Infrastructure-layer repository implementations (Domain write-side ->
//                  FinancialRecordAggregateDapperRepository, Application read-side ->
//                  FinancialRecordReadDapperRepository). Named
//                  "AddFinancialRecordAggregateInfrastructure" to avoid any collision with the
//                  pre-existing, unrelated "AddFinancialsRepositories" registration for the
//                  read-only Financials.Financials date-range vertical -- both are additive and
//                  called side-by-side from the central InfrastructureServiceRegistration.cs.
//                  Mirrors ComplianceAuditingInfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Financials.Persistence;

namespace PlantSafety.Infrastructure.Financials.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the new FinancialRecord aggregate module.</summary>
public static class FinancialRecordInfrastructureServiceRegistration
{
    /// <summary>Adds FinancialRecord Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddFinancialRecordAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Financials.Interfaces.IFinancialRecordRepository, FinancialRecordAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Financials.Repositories.IFinancialRecordDetailRepository, FinancialRecordReadDapperRepository>();

        return services;
    }
}
