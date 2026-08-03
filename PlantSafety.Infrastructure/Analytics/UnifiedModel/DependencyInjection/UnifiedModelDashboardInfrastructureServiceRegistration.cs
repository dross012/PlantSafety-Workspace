// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Infrastructure
//  File:           UnifiedModelDashboardInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the six unified-analytics-model BI
//                  dashboard Infrastructure-layer repository interfaces (Rule: Phase 5 §4.3)
//                  against the single shared UnifiedModelDashboardDapperRepository
//                  implementation. NOT wired into the shared InfrastructureServiceRegistration.cs
//                  -- call <c>services.AddAnalyticsUnifiedModelDashboardInfrastructure()</c>
//                  from host startup, mirroring AnalyticsLegacyRepositoryRegistration and
//                  RiskManagementInfrastructureServiceRegistration. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Repositories;
using PlantSafety.Infrastructure.Analytics.UnifiedModel.Persistence;

namespace PlantSafety.Infrastructure.Analytics.UnifiedModel.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Analytics unified-model BI dashboards.</summary>
public static class UnifiedModelDashboardInfrastructureServiceRegistration
{
    /// <summary>Adds the six unified-model dashboard Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddAnalyticsUnifiedModelDashboardInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<UnifiedModelDashboardDapperRepository>();
        services.AddScoped<ISchedulingDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());
        services.AddScoped<IProductionDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());
        services.AddScoped<IMaintenanceDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());
        services.AddScoped<IRiskDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());
        services.AddScoped<IContinuousImprovementDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());
        services.AddScoped<IAuditDashboardRepository>(sp => sp.GetRequiredService<UnifiedModelDashboardDapperRepository>());

        return services;
    }
}
