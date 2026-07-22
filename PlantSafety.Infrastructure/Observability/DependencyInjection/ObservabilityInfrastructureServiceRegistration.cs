// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability
//  Layer:          Infrastructure
//  File:           ObservabilityInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Observability Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Observability.AutomatedIncidentDetection.Persistence;
using PlantSafety.Infrastructure.Observability.EnvironmentalAssetMonitoring.Persistence;
using PlantSafety.Infrastructure.Observability.FacilityTrafficMonitoring.Persistence;
using PlantSafety.Infrastructure.Observability.MachineProcessMonitoring.Persistence;
using PlantSafety.Infrastructure.Observability.PPEBehavioralMonitoring.Persistence;
using PlantSafety.Application.Observability.Commands;
using PlantSafety.Infrastructure.Observability.Commands.Persistence;

namespace PlantSafety.Infrastructure.Observability.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Observability module.</summary>
public static class ObservabilityInfrastructureServiceRegistration
{
    /// <summary>Adds Observability Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddObservabilityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Interfaces.IAutomatedIncidentDetectionRepository, AutomatedIncidentDetectionDapperRepository>();
        services.AddScoped<PlantSafety.Application.Observability.AutomatedIncidentDetection.Repositories.IAutomatedIncidentDetectionRepository, AutomatedIncidentDetectionReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Interfaces.IEnvironmentalAssetMonitoringRepository, EnvironmentalAssetMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Repositories.IEnvironmentalAssetMonitoringRepository, EnvironmentalAssetMonitoringReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Interfaces.IFacilityTrafficMonitoringRepository, FacilityTrafficMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Application.Observability.FacilityTrafficMonitoring.Repositories.IFacilityTrafficMonitoringRepository, FacilityTrafficMonitoringReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces.IMachineProcessMonitoringRepository, MachineProcessMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Application.Observability.MachineProcessMonitoring.Repositories.IMachineProcessMonitoringRepository, MachineProcessMonitoringReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Interfaces.IPPEBehavioralMonitoringRepository, PPEBehavioralMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Application.Observability.PPEBehavioralMonitoring.Repositories.IPPEBehavioralMonitoringRepository, PPEBehavioralMonitoringReadDapperRepository>();
        services.AddScoped<IObservabilityBulkRepository, ObservabilityBulkDapperRepository>();

        return services;
    }
}
