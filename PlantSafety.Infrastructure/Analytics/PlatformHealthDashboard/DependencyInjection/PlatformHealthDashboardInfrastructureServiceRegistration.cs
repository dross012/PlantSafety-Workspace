// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PlatformHealthDashboard
//  Layer:          Infrastructure
//  File:           PlatformHealthDashboardInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the PlatformHealthDashboard read-model
//                  repository and the request-metrics capture pipeline (channel recorder +
//                  hosted flush service) that feeds it. Wired directly into
//                  InfrastructureServiceRegistration.AddInfrastructure() (Handbook Phase 6 §5.3).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Observability;
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Repositories;
using PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Capture;
using PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Persistence;

namespace PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the PlatformHealthDashboard module.</summary>
public static class PlatformHealthDashboardInfrastructureServiceRegistration
{
    /// <summary>Adds the PlatformHealthDashboard repository and request-metrics capture pipeline to the service collection.</summary>
    public static IServiceCollection AddPlatformHealthDashboardInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPlatformHealthDashboardRepository, PlatformHealthDashboardDapperRepository>();

        // Singleton: the channel is the shared hand-off point between every scoped HTTP request
        // (writer, via IRequestMetricsRecorder) and the singleton hosted drain service (reader).
        services.AddSingleton<ChannelRequestMetricsRecorder>();
        services.AddSingleton<IRequestMetricsRecorder>(sp => sp.GetRequiredService<ChannelRequestMetricsRecorder>());
        services.AddHostedService<RequestMetricsCaptureService>();

        return services;
    }
}
