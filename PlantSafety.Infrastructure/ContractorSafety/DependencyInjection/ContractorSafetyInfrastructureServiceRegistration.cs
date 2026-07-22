// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           ContractorSafetyInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the ContractorSafety Infrastructure
//                  layer (5 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddContractorSafetyInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by
//                  the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.Repositories;
using PlantSafety.Infrastructure.ContractorSafety.Persistence;
using PlantSafety.Infrastructure.ContractorSafety.ActiveMonitoringAuditing.Persistence;
using PlantSafety.Infrastructure.ContractorSafety.IncidentTrackingPerformanceEvaluation.Persistence;
using PlantSafety.Infrastructure.ContractorSafety.PermitToWork.Persistence;
using PlantSafety.Infrastructure.ContractorSafety.PreQualificationVetting.Persistence;
using PlantSafety.Infrastructure.ContractorSafety.SiteOrientation.Persistence;

namespace PlantSafety.Infrastructure.ContractorSafety.DependencyInjection;

/// <summary>Per-module DI entry point for the ContractorSafety Infrastructure layer.</summary>
public static class ContractorSafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all ContractorSafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddContractorSafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Interfaces.IActiveMonitoringAuditingRepository, ActiveMonitoringAuditingDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Repositories.IActiveMonitoringAuditingRepository, ActiveMonitoringAuditingDapperRepository>();
        services.AddScoped<PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Interfaces.IIncidentTrackingPerformanceEvaluationRepository, IncidentTrackingPerformanceEvaluationDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Repositories.IIncidentTrackingPerformanceEvaluationRepository, IncidentTrackingPerformanceEvaluationDapperRepository>();
        services.AddScoped<PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces.IPermitToWorkRepository, PermitToWorkDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorSafety.PermitToWork.Repositories.IPermitToWorkRepository, PermitToWorkDapperRepository>();
        services.AddScoped<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Interfaces.IPreQualificationVettingRepository, PreQualificationVettingDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorSafety.PreQualificationVetting.Repositories.IPreQualificationVettingRepository, PreQualificationVettingDapperRepository>();
        services.AddScoped<PlantSafety.Domain.ContractorSafety.SiteOrientation.Interfaces.ISiteOrientationRepository, SiteOrientationDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorSafety.SiteOrientation.Repositories.ISiteOrientationRepository, SiteOrientationDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<IContractorSafetyRepository, ContractorSafetyRepository>();

        return services;
    }
}
