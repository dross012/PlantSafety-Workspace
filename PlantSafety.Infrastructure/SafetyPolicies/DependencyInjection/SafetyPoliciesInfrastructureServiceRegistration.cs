// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Infrastructure
//  File:           SafetyPoliciesInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the SafetyPolicies Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Repositories;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Repositories;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Repositories;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Repositories;
using PlantSafety.Application.SafetyPolicies.HotWork.Repositories;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Repositories;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Repositories;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Repositories;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Repositories;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Repositories;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Repositories;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Repositories;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Repositories;
using PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Repositories;
using PlantSafety.Domain.SafetyPolicies.Interfaces;
using PlantSafety.Infrastructure.SafetyPolicies.Persistence;

namespace PlantSafety.Infrastructure.SafetyPolicies.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the SafetyPolicies module.</summary>
public static class SafetyPoliciesInfrastructureServiceRegistration
{
    /// <summary>Adds SafetyPolicies Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddSafetyPoliciesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISafetyPolicyRepository, SafetyPolicyDapperRepository>();

        services.AddScoped<SafetyPoliciesCategoryDapperRepository>();
        services.AddScoped<IConfinedSpaceEntryRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IContractorSafetyRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IElectricalSafetyRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IEmergencyResponseEvacuationRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IEnvironmentalChemicalSafetyRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IEquipmentMachineOperationRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IHazardCommunicationAssessmentRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IHotWorkRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IHousekeepingErgonomicsRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IIncidentInvestigationCorrectiveActionRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IMachineMaintenanceInspectionRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IMaterialHandlingStorageRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IPersonalProtectiveEquipmentRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<ISafetyCultureBehavioralRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<ISafetyTrainingReportingRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<ISecurityAccessControlRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());
        services.AddScoped<IWorkingAtHeightsRepository>(sp => sp.GetRequiredService<SafetyPoliciesCategoryDapperRepository>());

        return services;
    }
}
