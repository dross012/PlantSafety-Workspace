// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           CyberPhysicalSecurityInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the CyberPhysicalSecurity Infrastructure-
//                  layer repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally, mirroring the RiskManagement/PermitToWork precedent.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;
using PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the CyberPhysicalSecurity module.</summary>
public static class CyberPhysicalSecurityInfrastructureServiceRegistration
{
    /// <summary>Adds CyberPhysicalSecurity Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddCyberPhysicalSecurityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<OtAssetDapperRepository>();
        services.AddScoped<IOtAssetRepository>(sp => sp.GetRequiredService<OtAssetDapperRepository>());
        services.AddScoped<IOtAssetReadRepository>(sp => sp.GetRequiredService<OtAssetDapperRepository>());

        services.AddScoped<ControlChangeEventDapperRepository>();
        services.AddScoped<IControlChangeEventRepository>(sp => sp.GetRequiredService<ControlChangeEventDapperRepository>());
        services.AddScoped<IControlChangeEventReadRepository>(sp => sp.GetRequiredService<ControlChangeEventDapperRepository>());

        services.AddScoped<SensorIntegrityCheckDapperRepository>();
        services.AddScoped<ISensorIntegrityCheckRepository>(sp => sp.GetRequiredService<SensorIntegrityCheckDapperRepository>());
        services.AddScoped<ISensorIntegrityCheckReadRepository>(sp => sp.GetRequiredService<SensorIntegrityCheckDapperRepository>());

        services.AddScoped<SabotageIndicatorDapperRepository>();
        services.AddScoped<ISabotageIndicatorRepository>(sp => sp.GetRequiredService<SabotageIndicatorDapperRepository>());
        services.AddScoped<ISabotageIndicatorReadRepository>(sp => sp.GetRequiredService<SabotageIndicatorDapperRepository>());

        services.AddScoped<CyberPhysicalIncidentDapperRepository>();
        services.AddScoped<ICyberPhysicalIncidentRepository>(sp => sp.GetRequiredService<CyberPhysicalIncidentDapperRepository>());
        services.AddScoped<ICyberPhysicalIncidentReadRepository>(sp => sp.GetRequiredService<CyberPhysicalIncidentDapperRepository>());

        services.AddScoped<PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces.ICyberPhysicalIncidentFactory,
            PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Factories.CyberPhysicalIncidentFactory>();

        return services;
    }
}
