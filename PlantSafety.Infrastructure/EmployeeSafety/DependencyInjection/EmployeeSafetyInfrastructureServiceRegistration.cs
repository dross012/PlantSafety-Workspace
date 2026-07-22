// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           EmployeeSafetyInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the EmployeeSafety Infrastructure
//                  layer (7 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddEmployeeSafetyInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by
//                  the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.Repositories;
using PlantSafety.Infrastructure.EmployeeSafety.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.EmergencyPreparedness.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.EnvironmentalHealthMonitoring.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.ErgonomicsAndMaterialHandling.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.HazardCommunicationTraining.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.LotoPrograms.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.MachineGuarding.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.Ppe.Persistence;

namespace PlantSafety.Infrastructure.EmployeeSafety.DependencyInjection;

/// <summary>Per-module DI entry point for the EmployeeSafety Infrastructure layer.</summary>
public static class EmployeeSafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all EmployeeSafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddEmployeeSafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Interfaces.IEmergencyPreparednessRepository, EmergencyPreparednessDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Repositories.IEmergencyPreparednessRepository, EmergencyPreparednessDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Interfaces.IEnvironmentalHealthMonitoringRepository, EnvironmentalHealthMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Repositories.IEnvironmentalHealthMonitoringRepository, EnvironmentalHealthMonitoringDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Interfaces.IErgonomicsAndMaterialHandlingRepository, ErgonomicsAndMaterialHandlingDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Repositories.IErgonomicsAndMaterialHandlingRepository, ErgonomicsAndMaterialHandlingDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces.IHazardCommunicationTrainingRepository, HazardCommunicationTrainingDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Repositories.IHazardCommunicationTrainingRepository, HazardCommunicationTrainingDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces.ILotoProgramsRepository, LotoProgramsDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.LotoPrograms.Repositories.ILotoProgramsRepository, LotoProgramsDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Interfaces.IMachineGuardingRepository, MachineGuardingDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.MachineGuarding.Repositories.IMachineGuardingRepository, MachineGuardingDapperRepository>();
        services.AddScoped<PlantSafety.Domain.EmployeeSafety.Ppe.Interfaces.IPpeRepository, PpeDapperRepository>();
        services.AddScoped<PlantSafety.Application.EmployeeSafety.Ppe.Repositories.IPpeRepository, PpeDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<IEmployeeSafetyRepository, EmployeeSafetyRepository>();

        return services;
    }
}
