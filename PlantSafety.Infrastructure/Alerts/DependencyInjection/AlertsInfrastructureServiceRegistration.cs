// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts
//  Layer:          Infrastructure
//  File:           AlertsInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Per-module DI registration entry point for the Alerts.Alert Infrastructure
//      implementations. Deliberately kept separate from the shared
//      PlantSafety.Infrastructure.DependencyInjection.InfrastructureServiceRegistration so that
//      module additions never require editing the shared file. Call
//      services.AddAlertsInfrastructure() from the host composition root alongside
//      services.AddInfrastructure(configuration).
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Alerts.Commands;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Domain.Alerts.Alert.Interfaces;
using PlantSafety.Infrastructure.Alerts.Alert.Persistence;

namespace PlantSafety.Infrastructure.Alerts.DependencyInjection;

/// <summary>Canonical DI entry point for the Alerts.Alert Infrastructure module.</summary>
public static class AlertsInfrastructureServiceRegistration
{
    /// <summary>Registers all Alerts.Alert Infrastructure services into the DI container.</summary>
    public static IServiceCollection AddAlertsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<AlertDapperRepository>();
        services.AddScoped<IAlertRepository>(sp => sp.GetRequiredService<AlertDapperRepository>());
        services.AddScoped<IAlertCommandRepository>(sp => sp.GetRequiredService<AlertDapperRepository>());
        services.AddScoped<IAlertQueryRepository, AlertQueryDapperRepository>();

        return services;
    }
}
