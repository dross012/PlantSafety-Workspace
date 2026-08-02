// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Infrastructure
//  File:           RiskManagementInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the RiskManagement Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- registers RiskReadDapperRepository against
//                  Application.RiskManagement.Repositories.IRiskRepository (Phase 4 read-model
//                  gap remediation). That interface shares its simple name with
//                  Domain.RiskManagement.Interfaces.IRiskRepository already registered below --
//                  both registration lines use fully-qualified names rather than a `using` for the
//                  Application repositories namespace, to avoid an ambiguous-reference compile
//                  error. Mirrors PermitToWorkInfrastructureServiceRegistration's identical
//                  same-name/different-namespace precedent.
// ============================================================================
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Repositories;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Repositories;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Repositories;
using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Repositories;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Repositories;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Repositories;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Repositories;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Repositories;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Repositories;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Repositories;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Repositories;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Infrastructure.RiskManagement.Persistence;

namespace PlantSafety.Infrastructure.RiskManagement.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the RiskManagement module.</summary>
public static class RiskManagementInfrastructureServiceRegistration
{
    /// <summary>Adds RiskManagement Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddRiskManagementInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IRiskRepository, RiskDapperRepository>();
        services.AddScoped<PlantSafety.Application.RiskManagement.Repositories.IRiskRepository, RiskReadDapperRepository>();

        services.AddScoped<RiskManagementCategoryDapperRepository>();
        services.AddScoped<IChemicalEnvironmentalSafetyRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IContinuousMonitoringReviewRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IControlMeasuresHierarchyRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IEmergencyPreparednessRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IErgonomicsMaterialHandlingRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IHazardIdentificationMeasurementRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<ILotoConfinedSpacesRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IMachineOperationalGuardingRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IPreStartupSafetyReviewRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<IProcessSafetyInformationRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());
        services.AddScoped<ITrainingContractorManagementRepository>(sp => sp.GetRequiredService<RiskManagementCategoryDapperRepository>());

        return services;
    }
}
