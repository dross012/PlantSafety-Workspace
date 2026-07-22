// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.DependencyInjection
//  Layer:          Infrastructure
//  File:           SafetyProceduresInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the SafetyProcedures Infrastructure
//                  layer (8 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddSafetyProceduresInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by
//                  the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.Repositories;
using PlantSafety.Infrastructure.SafetyProcedures.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.EmergencyResponsePlanning.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.ErgonomicsWorkstationDesign.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.HazardCommunicationRightToKnow.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.LotoProcedures.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.MachineGuardingInspections.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.MaterialHandlingStorage.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.PpePolicies.Persistence;
using PlantSafety.Infrastructure.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Persistence;

namespace PlantSafety.Infrastructure.SafetyProcedures.DependencyInjection;

/// <summary>Per-module DI entry point for the SafetyProcedures Infrastructure layer.</summary>
public static class SafetyProceduresInfrastructureServiceRegistration
{
    /// <summary>Registers all SafetyProcedures Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddSafetyProceduresInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Interfaces.IEmergencyResponsePlanningRepository, EmergencyResponsePlanningDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Repositories.IEmergencyResponsePlanningRepository, EmergencyResponsePlanningDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Interfaces.IErgonomicsWorkstationDesignRepository, ErgonomicsWorkstationDesignDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Repositories.IErgonomicsWorkstationDesignRepository, ErgonomicsWorkstationDesignDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Interfaces.IHazardCommunicationRightToKnowRepository, HazardCommunicationRightToKnowDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Repositories.IHazardCommunicationRightToKnowRepository, HazardCommunicationRightToKnowDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Interfaces.ILotoProceduresRepository, LotoProceduresDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.LotoProcedures.Repositories.ILotoProceduresRepository, LotoProceduresDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Interfaces.IMachineGuardingInspectionsRepository, MachineGuardingInspectionsDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Repositories.IMachineGuardingInspectionsRepository, MachineGuardingInspectionsDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Interfaces.IMaterialHandlingStorageRepository, MaterialHandlingStorageDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Repositories.IMaterialHandlingStorageRepository, MaterialHandlingStorageDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.PpePolicies.Interfaces.IPpePoliciesRepository, PpePoliciesDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.PpePolicies.Repositories.IPpePoliciesRepository, PpePoliciesDapperRepository>();
        services.AddScoped<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Interfaces.IRoutineSafetyAuditsIncidentReportingRepository, RoutineSafetyAuditsIncidentReportingDapperRepository>();
        services.AddScoped<PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Repositories.IRoutineSafetyAuditsIncidentReportingRepository, RoutineSafetyAuditsIncidentReportingDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<ISafetyProceduresRepository, SafetyProceduresRepository>();

        return services;
    }
}
