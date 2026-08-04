// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Infrastructure
//  File:           SustainabilityAdvancedMetricsInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Sustainability.AdvancedMetrics
//                  Infrastructure-layer repository implementations: the two Domain-layer
//                  write-side repositories (IAdvancedMetricsLedgerRepository,
//                  IEsgDisclosureReportRepository) and the Application-layer read-side
//                  repository (IAdvancedMetricsReadRepository). Mirrors
//                  PlantSafety.Infrastructure.Sustainability.DependencyInjection.
//                  SustainabilityAggregateInfrastructureServiceRegistration's naming/shape
//                  precedent for a sub-module sitting alongside the generic Sustainability
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.Persistence;

namespace PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Sustainability.AdvancedMetrics sub-module.</summary>
public static class SustainabilityAdvancedMetricsInfrastructureServiceRegistration
{
    /// <summary>Adds Sustainability.AdvancedMetrics Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddSustainabilityAdvancedMetricsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces.IAdvancedMetricsLedgerRepository, AdvancedMetricsLedgerDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces.IEsgDisclosureReportRepository, EsgDisclosureReportDapperRepository>();
        services.AddScoped<PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories.IAdvancedMetricsReadRepository, AdvancedMetricsReadDapperRepository>();

        return services;
    }
}
