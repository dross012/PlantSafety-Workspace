// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards
//  Layer:          Infrastructure
//  File:           DashboardsInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Per-module DI registration entry point for the Dashboards.Dashboard and Dashboards.Widget
//      Infrastructure implementations. Deliberately kept separate from the shared
//      PlantSafety.Infrastructure.DependencyInjection.InfrastructureServiceRegistration so that
//      module additions never require editing the shared file. Call
//      services.AddDashboardsInfrastructure() from the host composition root alongside
//      services.AddInfrastructure(configuration).
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Dashboards.Dashboard.Persistence;
using PlantSafety.Infrastructure.Dashboards.Widget.Persistence;
using DomainIDashboardRepository = PlantSafety.Domain.Dashboards.Dashboard.Interfaces.IDashboardRepository;
using AppIDashboardRepository = PlantSafety.Application.Dashboards.Dashboard.Repositories.IDashboardRepository;
using DomainIWidgetRepository = PlantSafety.Domain.Dashboards.Widget.Interfaces.IWidgetRepository;
using AppIWidgetRepository = PlantSafety.Application.Dashboards.Widget.Repositories.IWidgetRepository;

namespace PlantSafety.Infrastructure.Dashboards.DependencyInjection;

/// <summary>Canonical DI entry point for the Dashboards.Dashboard and Dashboards.Widget Infrastructure modules.</summary>
public static class DashboardsInfrastructureServiceRegistration
{
    /// <summary>Registers all Dashboards Infrastructure services into the DI container.</summary>
    public static IServiceCollection AddDashboardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<DashboardDapperRepository>();
        services.AddScoped<DomainIDashboardRepository>(sp => sp.GetRequiredService<DashboardDapperRepository>());
        services.AddScoped<AppIDashboardRepository>(sp => sp.GetRequiredService<DashboardDapperRepository>());

        services.AddScoped<WidgetDapperRepository>();
        services.AddScoped<DomainIWidgetRepository>(sp => sp.GetRequiredService<WidgetDapperRepository>());
        services.AddScoped<AppIWidgetRepository>(sp => sp.GetRequiredService<WidgetDapperRepository>());

        return services;
    }
}
