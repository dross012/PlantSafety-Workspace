// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DependencyInjection
//  Layer:          Infrastructure
//  File:           WaterDischargeComplianceInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-sub-domain DI registration entry point for the WaterDischargeCompliance
//                  Infrastructure layer (6 entity repositories: DischargePermit, DischargePoint,
//                  EffluentSample, DischargeLimit, LimitExceedance, DischargeMonitoringReport),
//                  each implementing both its Domain write-repository interface and its
//                  Application read-model repository interface against the same Dapper
//                  repository class. Call
//                  <c>services.AddWaterDischargeComplianceInfrastructure()</c> alongside
//                  <c>services.AddEnvironmentalHealthAggregateInfrastructure()</c> in
//                  InfrastructureServiceRegistration.AddInfrastructure(). Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Persistence;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Persistence;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Persistence;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Persistence;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Persistence;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Persistence;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DependencyInjection;

/// <summary>Per-sub-domain DI entry point for the WaterDischargeCompliance Infrastructure layer.</summary>
public static class WaterDischargeComplianceInfrastructureServiceRegistration
{
    /// <summary>Registers all WaterDischargeCompliance Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddWaterDischargeComplianceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces.IDischargePermitRepository, DischargePermitDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories.IDischargePermitRepository, DischargePermitDapperRepository>();

        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces.IDischargePointRepository, DischargePointDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Repositories.IDischargePointRepository, DischargePointDapperRepository>();

        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces.IEffluentSampleRepository, EffluentSampleDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Repositories.IEffluentSampleRepository, EffluentSampleDapperRepository>();

        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces.IDischargeLimitRepository, DischargeLimitDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Repositories.IDischargeLimitRepository, DischargeLimitDapperRepository>();

        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces.ILimitExceedanceRepository, LimitExceedanceDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories.ILimitExceedanceRepository, LimitExceedanceDapperRepository>();

        services.AddScoped<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces.IDischargeMonitoringReportRepository, DischargeMonitoringReportDapperRepository>();
        services.AddScoped<PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Repositories.IDischargeMonitoringReportRepository, DischargeMonitoringReportDapperRepository>();

        return services;
    }
}