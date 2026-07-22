// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.DependencyInjection
//  Layer:          Infrastructure
//  File:           ChecklistsInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the Checklists Infrastructure
//                  layer (2 Domain aggregate repositories + 8 Application read repositories).
//                  Call <c>services.AddChecklistsInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked
//                  by the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Repositories;
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Repositories;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;
using PlantSafety.Infrastructure.Checklists.ChecklistCompletion.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.ContractorSafety.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.HotWork.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.Persistence;
using PlantSafety.Infrastructure.Checklists.ChecklistTemplate.WorkingAtHeights.Persistence;

namespace PlantSafety.Infrastructure.Checklists.DependencyInjection;

/// <summary>Per-module DI entry point for the Checklists Infrastructure layer.</summary>
public static class ChecklistsInfrastructureServiceRegistration
{
    /// <summary>Registers all Checklists Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddChecklistsInfrastructure(this IServiceCollection services)
    {
        // Domain aggregate repositories
        services.AddScoped<IChecklistTemplateRepository, ChecklistTemplateDapperRepository>();
        services.AddScoped<IChecklistCompletionRepository, ChecklistCompletionDapperRepository>();

        // Application category read repositories
        services.AddScoped<IConfinedSpaceEntryRepository, ConfinedSpaceEntryDapperRepository>();
        services.AddScoped<IContractorSafetyRepository, ContractorSafetyDapperRepository>();
        services.AddScoped<IEmergencySystemsAndLifeSafetyRepository, EmergencySystemsAndLifeSafetyDapperRepository>();
        services.AddScoped<IHazardousMaterialsAndChemicalSafetyRepository, HazardousMaterialsAndChemicalSafetyDapperRepository>();
        services.AddScoped<IHotWorkRepository, HotWorkDapperRepository>();
        services.AddScoped<IMachineGuardingAndLotoRepository, MachineGuardingAndLotoDapperRepository>();
        services.AddScoped<IPersonalProtectiveEquipmentRepository, PersonalProtectiveEquipmentDapperRepository>();
        services.AddScoped<IWorkingAtHeightsRepository, WorkingAtHeightsDapperRepository>();

        return services;
    }
}
